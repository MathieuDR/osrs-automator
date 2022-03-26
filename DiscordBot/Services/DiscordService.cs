using System.Text;
using Common.Extensions;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Helpers.Extensions;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.ClanFunds;
using DiscordBot.Common.Models.Data.Drops;
using WiseOldManConnector.Helpers;
using Player = DiscordBot.Common.Models.Data.PlayerManagement.Player;

namespace DiscordBot.Services;

public class DiscordService : IDiscordService {
    private readonly DiscordSocketClient _client;
    private readonly ILogger<DiscordService> _logger;

    public DiscordService(ILogger<DiscordService> logger, DiscordSocketClient client) {
        _logger = logger;
        _client = client;
    }

    public async Task<Result> SetUsername(GuildUser user, string nickname) {
        var guild = _client.GetGuild(user.GuildId.UlongValue);
        if (guild == null) {
            return Result.Fail($"Cannot find guild with id {user.GuildId}");
        }

        var discordUser = guild.GetUser(user.Id.UlongValue);
        if (discordUser == null) {
            return Result.Fail($"Cannot find user in guild with id {user.Id}");
        }

        await discordUser.ModifyAsync(x => x.Nickname = nickname);
        return Result.Ok();
    }

    public async Task<Result> PrintRunescapeDataDrop(RunescapeDropData data, DiscordGuildId guildId, DiscordChannelId channelId) {
        var imagesArr = data.DistinctImages.ToArray();

        var channel = _client.GetGuild(guildId.UlongValue).GetTextChannel(channelId.UlongValue);

        await channel.SendMessageAsync(
            $"New automated drop handled. Drops: {data.Drops.Count()} ({Math.Max(data.TotalValue, data.TotalHaValue)}), images: {imagesArr.Count()}");

        await channel.SendMessageAsync(
            $"drops: {string.Join(", ", data.Drops.Select(x => $"{x.Item.Name} x{x.Amount} ({Math.Max(x.TotalValue, x.TotalHaValue)})"))}");

        for (var i = 0; i < imagesArr.Length; i++) {
            var image = imagesArr[i];
            var stream = ToStream(image);
            await channel.SendFileAsync(stream, "image.png", $"Image {i}/{imagesArr.Count()}");
        }

        return Result.Ok();
    }

    public Task<Result<IEnumerable<Guild>>> GetGuilds() {
        var result = Result.Ok(_client.Guilds.Select(x=>x.ToGuildDto()));
        return Task.FromResult(result);
    }

    public Task<Result<IEnumerable<Channel>>> GetChannelsForGuild(DiscordGuildId guildId) {
        var result = _client.GetGuild(guildId.UlongValue).Channels.Select(x => x.ToChannelDto());
        return Task.FromResult(Result.Ok(result));
    }
    
    public Task<Result<Dictionary<Channel, IEnumerable<Channel>>>>GetNestedChannelsForGuild(DiscordGuildId guildId) {
        var result = _client.GetGuild(guildId.UlongValue).Channels.Select(x => x.ToChannelDto());
        return Task.FromResult(Result.Ok(result.NestChannels()));
    }
    
    public Task<Result<IEnumerable<GuildUser>>> GetUsers(DiscordGuildId guildId) {
        var result = _client.GetGuild(guildId.UlongValue).Users.Select(x => x.ToGuildUserDto());
        return Task.FromResult(Result.Ok(result));
    }

    public async Task<Result> SendFailedEmbed(DiscordChannelId channelId, string message, Guid traceId) {
        EmbedBuilder builder = new EmbedBuilder();

        builder
            .AddCommonProperties()
            .WithDescription(message).WithTitle($"Failed to update group.")
            .AddField("TraceId", traceId.ToString());

        return await SendEmbed(channelId, builder);
    }

    public async Task<Result> SendWomGroupSuccessEmbed(DiscordChannelId channelId, string message, int groupId, string groupName) {
        EmbedBuilder builder = new EmbedBuilder();

        builder
            .AddWiseOldMan(groupName, $"https://wiseoldman.net/groups/{groupId}")
            .AddCommonProperties()
            .WithDescription(message).WithTitle($"Group {groupName} updated");

        return await SendEmbed(channelId, builder);
    }
    
    private async Task<Result> SendEmbed(DiscordChannelId channelId, EmbedBuilder builder) {
        var channel = await _client.GetChannelAsync(channelId.UlongValue);
        
        if(channel is ISocketMessageChannel socketChannel) {
            await socketChannel.SendMessageAsync("", false, builder.Build());
            return Result.Ok();
        }

        return Result.Fail("Could not send message");
    }


    public async Task<Result> MessageLeaderboards<T>(DiscordChannelId channelId, IEnumerable<MetricTypeLeaderboard<T>> leaderboards)
        where T : ILeaderboardMember {
        var channelTask = _client.GetChannelAsync(channelId.UlongValue);
        var metricMessages = leaderboards.Select(leaderboard => GetMessageForLeaderboard(leaderboard)).ToList();

        var toSendResult = CreateCompoundedMessagesForMultipleMessages(metricMessages);
        if (toSendResult.IsFailed) {
            return toSendResult.ToResult();
        }

        var channel = (await channelTask).As<ISocketMessageChannel>();
        foreach (var message in toSendResult.Value) {
            await channel.SendMessageAsync(message);
        }

        return Result.Ok();
    }

    public Task<Result> TrackClanFundEvent(DiscordGuildId guildId, ClanFundEvent clanFundEvent, DiscordChannelId clanFundsChannelId, long clanFundsTotalFunds) {
        var channel = _client.GetGuild(guildId.UlongValue).GetTextChannel(clanFundsChannelId.UlongValue);

        var embed = new EmbedBuilder()
            .AddCommonProperties()
            .WithTitle($"Clan funds updated")
            .AddField("Type", clanFundEvent.EventType.ToDisplayNameOrFriendly(), true)
            .AddField("Amount", clanFundEvent.Amount.FormatConditionally(), true);

        string verb = clanFundEvent.EventType switch {
            ClanFundEventType.Deposit => "From",
            ClanFundEventType.Withdraw => "To",
            ClanFundEventType.Donation => "From",
            ClanFundEventType.Refund => "To",
            ClanFundEventType.Other => "About",
            ClanFundEventType.System => "About",
            _ => "About"
        };

        if (clanFundEvent.PlayerId == DiscordUserId.Empty) {
            embed.AddField(verb, _client.GetGuild(guildId.UlongValue).GetUser(clanFundEvent.PlayerId.UlongValue)?.DisplayName() ?? clanFundEvent.PlayerName ?? _client.GetUser(clanFundEvent.PlayerId.UlongValue)?.DisplayName() ?? "Unknown", true);
        }

        if (!string.IsNullOrWhiteSpace(clanFundEvent.Reason)) {
            embed.AddField("Reason", clanFundEvent.Reason);
        }
        
        embed.AddField("Total funds", clanFundsTotalFunds.FormatNumber());
        embed.WithFooter(clanFundEvent.Id.ToString());
        
        var author = _client.GetUser(clanFundEvent.CreatorId.UlongValue);
        embed.WithAuthor($"Authorized by {author.Username}", author.GetAvatarUrl());
        
        _ = channel.SendMessageAsync(embed: embed.Build());
        return Task.FromResult(Result.Ok()); 
    }

    public async Task<Result<DiscordMessageId>> UpdateDonationMessage(DiscordGuildId guildId, DiscordChannelId clanFundsDonationLeaderBoardChannel, DiscordMessageId clanFundsDonationLeaderBoardMessage,
        IEnumerable<(DiscordUserId Player, string PlayerName, long Amount)> topDonations) {
        var donations = topDonations.ToList();
        var guild = _client.GetGuild(guildId.UlongValue);
        
        var leaderboard = new DiscordLeaderBoard<string>() {
            Name = "Top donations",
            Description = "Top donations in the clan funds",
            ScoreFieldName = "Amount",
            Entries = donations.Select((x,i) => new LeaderboardEntry<string>(guild.GetUser(x.Player.UlongValue)?.Mention ?? x.PlayerName ?? _client.GetUser(x.Player.UlongValue)?.DisplayName() ?? "Unknown", x.Amount.FormatNumber(), i + 1)).ToList()
        };
        
        // send message
        var message = await guild.GetTextChannel(clanFundsDonationLeaderBoardChannel.UlongValue).SendMessageAsync(leaderboard.ToMessage(2000));

        if (message.Id != default(ulong)) {
            await DeleteMessage(guildId, clanFundsDonationLeaderBoardChannel, clanFundsDonationLeaderBoardMessage);
            return Result.Ok(message.GetMessageId());
        }
        
        return Result.Fail("Could not send message / retrieve message id");
    }

    private async Task DeleteMessage(DiscordGuildId guildId, DiscordChannelId channelId, DiscordMessageId messageId) {
        // delete old message
        if (messageId == default) {
            return;
        }
        
        var channel = _client.GetGuild(guildId.UlongValue).GetTextChannel(channelId.UlongValue);
        var message = (await channel.GetMessageAsync(messageId.UlongValue)).As<IUserMessage>();
        if (message != null) {
            await message.DeleteAsync();
        }
    }


    private string GetMessageForLeaderboard<T>(MetricTypeLeaderboard<T> leaderboard) where T : ILeaderboardMember {
        var builder = new StringBuilder();
        builder.Append($"**{leaderboard.MetricType.ToDisplayNameOrFriendly()}** - Leaderboard");

        if (leaderboard is MetricTypeAndPeriodLeaderboard<T> periodLeaderboard) {
            builder.Append($" for {periodLeaderboard.Period}");
        }

        builder.Append($"{Environment.NewLine}```");
        builder.Append(leaderboard.MembersToString(3));
        builder.Append($"```{Environment.NewLine}");
        return builder.ToString();
    }

    private Result<IEnumerable<string>> CreateCompoundedMessagesForMultipleMessages(IEnumerable<string> messages) {
        var compoundedMessages = new List<string>();
        var maxSize = 1990;
        var builder = new StringBuilder();
        foreach (var message in messages) {
            if (builder.Length + message.Length >= maxSize) {
                if (builder.Length > 0) {
                    compoundedMessages.Add(builder.ToString());
                }

                builder = new StringBuilder();
            }

            builder.Append(message);
        }

        // Last message
        if (builder.Length > 0) {
            compoundedMessages.Add(builder.ToString());
        }

        return Result.Ok((IEnumerable<string>)compoundedMessages);
    }

    private static Stream ToStream(string image) {
        var bytes = Convert.FromBase64String(image);
        return new MemoryStream(bytes);
    }
}
