using System.Text;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Counting;
using DiscordBot.Common.Models.Enums;
using MathieuDR.Common.Extensions;

namespace DiscordBot.Commands.Interactive;

public class CountConfigurationApplicationCommandHandler : ApplicationCommandHandler {
    private const string SetChannelSubCommandName = "set-channel";
    private const string ChannelOption = "channel";
    private const string ViewSubCommandName = "view";
    private const string AddThresholdSubCommandName = "add-threshold";
    private const string ThresholdOption = "threshold";
    private const string NameOption = "name";
    private const string RoleOption = "role";
    private const string ThresholdRemoveOption = ThresholdOption;
    private const string RemoveOption = "remove";
    private readonly ICounterService _counterService;

    public CountConfigurationApplicationCommandHandler(ILogger<CountConfigurationApplicationCommandHandler> logger,
        ICounterService counterService) :
        base("configuration-count", "Configure the count module", logger) {
        _counterService = counterService;
    }

    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanAdmin;

    private static string ThresholdFormat {
        get {
            var builder = new StringBuilder();
            builder.Append("#:");
            builder.Append("Name");
            builder.Append(", role");
            return builder.ToString();
        }
    }

    private ButtonBuilder GetRemoveButton => new ButtonBuilder()
        .WithCustomId(SubCommand(RemoveOption))
        .WithLabel("Delete")
        .WithStyle(ButtonStyle.Danger)
        .WithEmote(new Emoji("🗑️"));

    public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
        var subCommand = context.Options.First().Key;

        var result = subCommand switch {
            SetChannelSubCommandName => await SetChannelHandler(context),
            ViewSubCommandName => await ViewHandler(context),
            AddThresholdSubCommandName => await AddThresholdHandler(context),
            _ => Result.Fail("Did not find a correct handler")
        };

        return result;
    }

    private async Task<Result> AddThresholdHandler(ApplicationCommandContext context) {
        var value = (int)context.SubCommandOptions.GetOptionValue<long>(ThresholdOption);
        var role = context.SubCommandOptions.GetOptionValue<IRole>(RoleOption);
        var name = context.SubCommandOptions.GetOptionValue<string>(NameOption);

        try {
            if (!await _counterService.CreateThreshold(context.GuildUser.ToGuildUserDto(), value, name, role?.ToRoleDto())) {
                return Result.Fail("Could not set the channel");
            }
        } catch (Exception e) {
            return Result.Fail(new ExceptionalError(e));
        }

        await context.CreateReplyBuilder()
            .WithEmbedFrom("Success!", "Successfully created the threshold")
            .WithEphemeral()
            .RespondAsync();

        return Result.Ok();
    }

    private async Task<Result> ViewHandler(ApplicationCommandContext context) {
        IReadOnlyList<CountThreshold> thresholds;
        DiscordChannelId channelId;

        try {
            thresholds = await _counterService.GetThresholds(context.Guild.GetGuildId());
            channelId = await _counterService.GetChannelForGuild(context.Guild.GetGuildId());
        } catch (Exception e) {
            return Result.Fail(new ExceptionalError(e));
        }

        var thresholdInfo = thresholds.Select((x, i) => (Id: i.ToString(), Label: ThresholdToString(context, x))).ToList();

        var descriptionBuilder = new StringBuilder();
        descriptionBuilder.AppendLine($"**Output channel:** <#{channelId}>");
        descriptionBuilder.AppendLine();
        descriptionBuilder.AppendLine("**Thresholds:**");
        //descriptionBuilder.AppendLine(ThresholdFormat);
        foreach (var info in thresholdInfo) {
            descriptionBuilder.AppendLine(info.Label.WithMention);
        }

        var response = context
            .CreateReplyBuilder(true)
            .WithEmbedFrom($"Count settings for {context.Guild.Name}", descriptionBuilder.ToString());

        if (thresholdInfo.Any()) {
            response.WithSelectMenu(GetThresholdSelectMenu(thresholdInfo))
                .WithButtons(GetRemoveButton.WithDisabled(true));
        }

        await response.WithEphemeral().RespondAsync();
        return Result.Ok();
    }


    private SelectMenuBuilder GetThresholdSelectMenu(IEnumerable<(string Id, (string WithMention, string NoMention) Label)> thresholdInfo) {
        return new SelectMenuBuilder()
            .WithPlaceholder("Select threshold to delete")
            .WithCustomId(SubCommand(ThresholdRemoveOption))
            .WithOptions(
                thresholdInfo.Select(i =>
                        new SelectMenuOptionBuilder()
                            .WithLabel(i.Label.NoMention)
                            .WithValue(i.Id))
                    .ToList());
    }

    private (string WithMention, string NoMention) ThresholdToString<T>(BaseInteractiveContext<T> context, CountThreshold threshold)
        where T : SocketInteraction {
        var noMentionBuilder = new StringBuilder();

        noMentionBuilder.Append($"{threshold.Threshold}: ");
        var name = string.IsNullOrEmpty(threshold.Name) ? "-" : threshold.Name;
        noMentionBuilder.Append(name);

        if (!threshold.GivenRoleId.HasValue) {
            return (noMentionBuilder.ToString(), noMentionBuilder.ToString());
        }

        noMentionBuilder.Append(", ");
        var mentionBuilder = new StringBuilder(noMentionBuilder.ToString());

        var roleTemp = context.Guild.GetRole(threshold.GivenRoleId.Value.UlongValue)?.Name ?? "deleted role";
        noMentionBuilder.Append(roleTemp);

        roleTemp = threshold.GivenRoleId.Value.ToRole();
        mentionBuilder.Append(roleTemp);

        return (mentionBuilder.ToString(), noMentionBuilder.ToString());
    }

    private async Task<Result> SetChannelHandler(ApplicationCommandContext context) {
        var channel = context.SubCommandOptions.GetOptionValue<IChannel>(ChannelOption);
        var postToChannel = context.Guild.Channels.FirstOrDefault(x => x.Id == channel.Id).As<ISocketMessageChannel>();

        if (postToChannel is null) {
            return Result.Fail($"The channel {channel.Name} is unavailable or not a text channel!");
        }

        try {
            if (await _counterService.SetChannelForCounts(context.GuildUser.ToGuildUserDto(), channel.ToChannelDto())) {
                return Result.Fail("Could not set the channel");
            }
        } catch (Exception e) {
            return Result.Fail(new ExceptionalError(e));
        }

        await context.CreateReplyBuilder()
            .WithEmbedFrom("Success!", "Successfully set the channel.")
            .WithEphemeral()
            .RespondAsync();
        return Result.Ok();
    }

    public override async Task<Result> HandleComponentAsync(MessageComponentContext context) {
        var subCommand = context.CustomSubCommandId;
        var result = subCommand switch {
            RemoveOption => await HandleDelete(context),
            ThresholdRemoveOption => await HandleThresholdSelected(context),
            _ => Result.Fail("Could not find subcommand handler")
        };

        if (result.IsFailed) {
            // Empty message
            await context.UpdateAsync(null, null, null, null, null);
        }

        return result;
    }

    private async Task<Result> HandleThresholdSelected(MessageComponentContext context) {
        var originalMenu = (SelectMenuComponent)context.InnerContext.Message.Components.First().Components.First();
        var selected = context.SelectedMenuOptions.First();

        var components = new ComponentBuilder()
            .WithOriginalMenu(originalMenu, selected)
            .WithButton(GetRemoveButton);

        await context.UpdateAsync(component: components.Build());

        return Result.Ok();
    }

    private async Task<Result> HandleDelete(MessageComponentContext context) {
        IReadOnlyList<CountThreshold> thresholds;
        var selected = ((SelectMenuComponent)context.InnerContext.Message.Components.First().Components.First()).Options.First(x => x.IsDefault == true)
            .Value;

        try {
            await _counterService.RemoveThreshold(context.Guild.GetGuildId(), int.Parse(selected));
            thresholds = await _counterService.GetThresholds(context.Guild.GetGuildId());
        } catch (Exception e) {
            return Result.Fail(new ExceptionalError(e));
        }

        var thresholdInfo = thresholds.Select((x, i) => (Id: i.ToString(), Label: ThresholdToString(context, x))).ToList();

        var components = new ComponentBuilder()
            .WithSelectMenu(GetThresholdSelectMenu(thresholdInfo))
            .WithButton(GetRemoveButton);

        await context.UpdateAsync(component: components.Build());

        return Result.Ok();
    }

    protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
        builder
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(SetChannelSubCommandName)
                .WithDescription("Set the channel for threshold messages")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(ChannelOption, ApplicationCommandOptionType.Channel, "The channel to use, it must be a text channel", true)
            )
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(ViewSubCommandName)
                .WithDescription("View the set channel and all thresholds")
                .WithType(ApplicationCommandOptionType.SubCommand)
            )
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(AddThresholdSubCommandName)
                .WithDescription("Adds a new threshold")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(ThresholdOption, ApplicationCommandOptionType.Integer, "The value that triggers the threshold, inclusive", true)
                .AddOption(NameOption, ApplicationCommandOptionType.String, "The name of the threshold", true)
                .AddOption(RoleOption, ApplicationCommandOptionType.Role, "Optional role to give the players", false)
            );

        return Task.FromResult(builder);
    }
}
