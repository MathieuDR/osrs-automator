using DiscordBot.Common.Configuration;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Hosting;
using DiscordBot.Services.Helpers;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Jobs;
using DiscordBot.Services.Models;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace DiscordBot.ServicesTests.Jobs;

public class HostingReminderJobTests {
	private static readonly DateOnly DueOn = new(2026, 10, 1);

	public enum SentState { NotSent, SentForThisDueOn, SentForOlderDueOn }

	private static DateTime? ToSentDate(SentState state, DateOnly dueOn) => state switch {
		SentState.NotSent => null,
		SentState.SentForThisDueOn => HostingDates.ToStorage(dueOn),
		SentState.SentForOlderDueOn => HostingDates.ToStorage(dueOn.AddYears(-1)),
		_ => throw new ArgumentOutOfRangeException(nameof(state))
	};

	private static HostingStatus StatusWithDaysOverdue(int? daysOverdue, DateOnly? dueOn = null) =>
		new(new DiscordGuildId(1), true, null, daysOverdue is null ? null : dueOn ?? DueOn, daysOverdue, null);

	private static GuildHostingState StateWith(SentState upcomingSent, SentState dueSent, DateOnly? dueOn = null) => new() {
		GuildId = new DiscordGuildId(1),
		UpcomingReminderSentForDueOn = ToSentDate(upcomingSent, dueOn ?? DueOn),
		DueReminderSentForDueOn = ToSentDate(dueSent, dueOn ?? DueOn)
	};

	// --- Decide ---

	[Fact]
	public void Decide_NoDueOn_ReturnsNone() {
		var status = StatusWithDaysOverdue(null);
		var state = StateWith(SentState.NotSent, SentState.NotSent);

		HostingReminderJob.Decide(status, state).Should().Be(ReminderKind.None);
	}

	[Theory]
	[InlineData(-31, SentState.NotSent, SentState.NotSent, ReminderKind.None)]
	[InlineData(-30, SentState.NotSent, SentState.NotSent, ReminderKind.Upcoming)]
	[InlineData(-30, SentState.SentForThisDueOn, SentState.NotSent, ReminderKind.None)]
	[InlineData(-30, SentState.SentForOlderDueOn, SentState.NotSent, ReminderKind.Upcoming)]
	[InlineData(-1, SentState.NotSent, SentState.NotSent, ReminderKind.Upcoming)]
	[InlineData(0, SentState.NotSent, SentState.NotSent, ReminderKind.Due)]
	[InlineData(5, SentState.NotSent, SentState.SentForThisDueOn, ReminderKind.None)]
	[InlineData(5, SentState.NotSent, SentState.SentForOlderDueOn, ReminderKind.Due)]
	public void Decide_ReturnsExpectedKind(int daysOverdue, SentState upcomingSent, SentState dueSent, ReminderKind expected) {
		var status = StatusWithDaysOverdue(daysOverdue);
		var state = StateWith(upcomingSent, dueSent);

		HostingReminderJob.Decide(status, state).Should().Be(expected);
	}

	// --- DoWork ---

	private static (HostingReminderJob Job, IHostingService Hosting, IDiscordService Discord, DiscordChannelId Channel, BotTeamConfiguration Team)
		CreateJob() {
		var hosting = Substitute.For<IHostingService>();
		var discord = Substitute.For<IDiscordService>();
		var team = new BotTeamConfiguration { OwnerId = new DiscordUserId(999) };
		var channel = new DiscordChannelId(555);

		hosting.GetReminderChannel().Returns(Result.Ok<DiscordChannelId?>(channel));

		var job = new HostingReminderJob(NullLogger<HostingReminderJob>.Instance, hosting, discord, Options.Create(team));

		return (job, hosting, discord, channel, team);
	}

	private static Guild MakeGuild(ulong id, string name) => new() { GuildId = new DiscordGuildId(id), Name = name };

	[Fact]
	public async Task DoWork_NoReminderChannel_LogsAndReturnsOk() {
		var hosting = Substitute.For<IHostingService>();
		var discord = Substitute.For<IDiscordService>();
		hosting.GetReminderChannel().Returns(Result.Ok<DiscordChannelId?>(null));

		var job = new HostingReminderJob(NullLogger<HostingReminderJob>.Instance, hosting, discord,
			Options.Create(new BotTeamConfiguration()));

		var result = await job.DoWorkForTests();

		result.IsSuccess.Should().BeTrue();
		await discord.DidNotReceive().GetGuilds();
	}

	[Fact]
	public async Task DoWork_OneGuildDueOneGuildFine_SendsEmbedForDueGuildOnlyAndMarksDueOnly() {
		var (job, hosting, discord, channel, team) = CreateJob();

		var dueGuild = MakeGuild(1, "ClanA");
		var fineGuild = MakeGuild(2, "ClanB");

		discord.GetGuilds().Returns(Result.Ok<IEnumerable<Guild>>(new[] { dueGuild, fineGuild }));

		var dueState = new GuildHostingState { GuildId = dueGuild.Id };
		var fineState = new GuildHostingState { GuildId = fineGuild.Id };
		hosting.GetAllStates().Returns(new List<GuildHostingState> { dueState, fineState });

		var dueStatus = new HostingStatus(dueGuild.Id, true, null, DueOn, 0, null);
		var fineStatus = new HostingStatus(fineGuild.Id, true, null, DueOn.AddMonths(3), -90, null);
		hosting.GetStatus(dueGuild.Id, dueGuild.Name).Returns(dueStatus);
		hosting.GetStatus(fineGuild.Id, fineGuild.Name).Returns(fineStatus);

		discord.SendMentionEmbed(channel, team.OwnerId, Arg.Any<string>(), Arg.Any<EmbedFieldDto[]>(), Arg.Any<bool>())
			.Returns(Result.Ok());

		var result = await job.DoWorkForTests();

		result.IsSuccess.Should().BeTrue();

		await discord.Received(1).SendMentionEmbed(channel, team.OwnerId, "Hosting payments",
			Arg.Is<EmbedFieldDto[]>(f => f.Length == 1 && f[0].Name == "ClanA"), true);

		hosting.Received(1).MarkDueReminderSent(dueGuild.Id, DueOn);
		hosting.DidNotReceive().MarkUpcomingReminderSent(Arg.Any<DiscordGuildId>(), Arg.Any<DateOnly>());
		hosting.DidNotReceive().MarkDueReminderSent(fineGuild.Id, Arg.Any<DateOnly>());
	}

	[Fact]
	public async Task DoWork_SendFails_MarksNothing() {
		var (job, hosting, discord, channel, team) = CreateJob();

		var dueGuild = MakeGuild(1, "ClanA");
		discord.GetGuilds().Returns(Result.Ok<IEnumerable<Guild>>(new[] { dueGuild }));

		var dueState = new GuildHostingState { GuildId = dueGuild.Id };
		hosting.GetAllStates().Returns(new List<GuildHostingState> { dueState });

		var dueStatus = new HostingStatus(dueGuild.Id, true, null, DueOn, 0, null);
		hosting.GetStatus(dueGuild.Id, dueGuild.Name).Returns(dueStatus);

		discord.SendMentionEmbed(channel, team.OwnerId, Arg.Any<string>(), Arg.Any<EmbedFieldDto[]>(), Arg.Any<bool>())
			.Returns(Result.Fail("boom"));

		var result = await job.DoWorkForTests();

		result.IsFailed.Should().BeTrue();
		hosting.DidNotReceive().MarkDueReminderSent(Arg.Any<DiscordGuildId>(), Arg.Any<DateOnly>());
		hosting.DidNotReceive().MarkUpcomingReminderSent(Arg.Any<DiscordGuildId>(), Arg.Any<DateOnly>());
	}

	[Fact]
	public async Task DoWork_GuildWithoutState_IsSkipped() {
		var (job, hosting, discord, channel, team) = CreateJob();

		var orphanGuild = MakeGuild(1, "Orphan");
		discord.GetGuilds().Returns(Result.Ok<IEnumerable<Guild>>(new[] { orphanGuild }));
		hosting.GetAllStates().Returns(new List<GuildHostingState>());

		var result = await job.DoWorkForTests();

		result.IsSuccess.Should().BeTrue();
		hosting.DidNotReceive().GetStatus(Arg.Any<DiscordGuildId>(), Arg.Any<string>());
		await discord.DidNotReceive().SendMentionEmbed(Arg.Any<DiscordChannelId>(), Arg.Any<DiscordUserId>(), Arg.Any<string>(),
			Arg.Any<EmbedFieldDto[]>(), Arg.Any<bool>());
	}

	[Fact]
	public async Task DoWork_UpcomingGuild_SendsNonAlertEmbedAndMarksUpcomingOnly() {
		var (job, hosting, discord, channel, team) = CreateJob();

		var upcomingGuild = MakeGuild(1, "ClanA");
		discord.GetGuilds().Returns(Result.Ok<IEnumerable<Guild>>(new[] { upcomingGuild }));

		var upcomingState = new GuildHostingState { GuildId = upcomingGuild.Id };
		hosting.GetAllStates().Returns(new List<GuildHostingState> { upcomingState });

		var upcomingStatus = new HostingStatus(upcomingGuild.Id, true, null, DueOn, -30, null);
		hosting.GetStatus(upcomingGuild.Id, upcomingGuild.Name).Returns(upcomingStatus);

		discord.SendMentionEmbed(channel, team.OwnerId, Arg.Any<string>(), Arg.Any<EmbedFieldDto[]>(), Arg.Any<bool>())
			.Returns(Result.Ok());

		var result = await job.DoWorkForTests();

		result.IsSuccess.Should().BeTrue();

		await discord.Received(1).SendMentionEmbed(channel, team.OwnerId, "Hosting payments",
			Arg.Is<EmbedFieldDto[]>(f => f.Length == 1 && f[0].Name == "ClanA"), false);

		hosting.Received(1).MarkUpcomingReminderSent(upcomingGuild.Id, DueOn);
		hosting.DidNotReceive().MarkDueReminderSent(Arg.Any<DiscordGuildId>(), Arg.Any<DateOnly>());
	}
}
