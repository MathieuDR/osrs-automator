using System.Text;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Decorators;
using DiscordBot.Common.Models.Enums;
using TimeZoneNames;

namespace DiscordBot.Commands.Interactive;

public class ConfigureApplicationCommandHandler : ApplicationCommandHandler {
    private const string SetServerSubCommandName = "server";
    private const string ViewConfigurationSubCommand = "view";
    private const string TimeZoneOption = "timezone";
    private const string WomGroupId = "group-id";
    private const string WomVerificationCode = "verification-code";


    private readonly IGroupService _groupService;
    private readonly IDictionary<string, string> _timeZones;

    public ConfigureApplicationCommandHandler(ILogger<ConfigureApplicationCommandHandler> logger, IGroupService groupService) : base(
        "Configure", "Configure this bot for this server", logger) {
        _groupService = groupService;

        _timeZones = TZNames.GetDisplayNames("en-GB");
    }

    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanAdmin;


    public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
        var subCommand = context.Options.First().Key;

        var result = subCommand switch {
            SetServerSubCommandName => await SetConfiguration(context),
            ViewConfigurationSubCommand => await ViewHandler(context),
            _ => Result.Fail("Did not find a correct handler")
        };

        return result;
    }

    public override Task<Result> HandleAutocompleteAsync(AutocompleteCommandContext context) {
        if (context.SubCommand == SetServerSubCommandName && context.CurrentOptionName == TimeZoneOption) {
            Logger.LogInformation("Searching {tzCount} timezones for the string '{string}'", _timeZones.Count, context.CurrentOptionAsString);

            var opts = _timeZones.Where(x => x.Value.ToLowerInvariant().Contains(context.CurrentOptionAsString.ToLowerInvariant()))
                .Select(x => x.Value).ToList();

            Logger.LogInformation("Found the following {@opts}", opts);

            _ = context.RespondAsync(opts);
            return Task.FromResult(Result.Ok());
        }

        return Task.FromResult(Result.Fail("Could not find correct auto complete option"));
    }

    private async Task<Result> ViewHandler(ApplicationCommandContext context) {
        var config = await _groupService.GetSettingsDictionary(context.Guild.ToGuildDto());

        var builder = new StringBuilder();
        foreach (var kvp in config) {
            builder.AppendLine($"**{kvp.Key}:** {kvp.Value}");
        }

        await context
            .CreateReplyBuilder()
            .WithEmbedFrom("Configuration", builder.ToString(), x => x.AddCommonProperties())
            .WithEphemeral()
            .RespondAsync();

        return Result.Ok();
    }

    private async Task<Result> SetConfiguration(ApplicationCommandContext context) {
        var groupId = (int)context.SubCommandOptions.GetOptionValue<long>(WomGroupId);
        var verificationCode = context.SubCommandOptions.GetOptionValue<string>(WomVerificationCode);
        var timeZone = context.SubCommandOptions.GetOptionValue<string>(TimeZoneOption);


        if (groupId <= 0) {
            return Result.Fail("Group id must be higher then 0");
        }

        // Needs more verification
        if (string.IsNullOrEmpty(verificationCode)) {
            return Result.Fail("Group verification must be set");
        }

        ItemDecorator<Group> decoratedGroup;
        try {
            decoratedGroup = await _groupService.SetGroupForGuild(context.GuildUser.ToGuildUserDto(), groupId, verificationCode);
        } catch (Exception e) {
            return Result.Fail(new ExceptionalError(e));
        }

        var tzResult = Result.Ok();
        if (!string.IsNullOrWhiteSpace(timeZone)) {
            tzResult = await HandleNewTimezone(context.GuildUser.ToGuildUserDto(), timeZone);
        }

        await context.CreateReplyBuilder(true)
            .WithEmbedFrom("Success", $"Group set to {decoratedGroup.Item.Name}", builder => builder
                .AddWiseOldMan(decoratedGroup))
            .WithEphemeral()
            .RespondAsync();

        return tzResult;
    }

    private async Task<Result> HandleNewTimezone(GuildUser caller, string timeZone) {
        var key = _timeZones.FirstOrDefault(x => string.Equals(x.Value, timeZone, StringComparison.InvariantCultureIgnoreCase)).Key;

        if (key is null) {
            return Result.Fail("Cannot find the timezone: " + timeZone + ". Please use the auto complete feature.");
        }

        return await _groupService.SetTimeZone(caller, key);
    }

    protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
        builder
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(SetServerSubCommandName)
                .WithDescription("Set the basic WOM configuration for this bot")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(WomGroupId, ApplicationCommandOptionType.Integer, "The ID of your wise old man group", true)
                .AddOption(WomVerificationCode, ApplicationCommandOptionType.String, "The verification code of your group", true)
                .AddOption(TimeZoneOption, ApplicationCommandOptionType.String, "Set the timezone", true, isAutocomplete: true)
            )
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(ViewConfigurationSubCommand)
                .WithDescription("View the current configuration")
                .WithType(ApplicationCommandOptionType.SubCommand)
            );
        return Task.FromResult(builder);
    }
}
