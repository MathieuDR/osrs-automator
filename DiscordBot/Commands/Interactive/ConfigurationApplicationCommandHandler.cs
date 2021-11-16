using System.Text;
using DiscordBot.Common.Models.Decorators;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive;

public class ConfigurationApplicationCommandHandler : ApplicationCommandHandler {
    private const string SetServerSubCommandName = "server";
    private const string ViewConfigurationSubCommand = "view";
    private const string TimeZoneOption = "timezone";
    private const string WomGroupId = "group-id";
    private const string WomVerificationCode = "verification-code";
    
    private readonly IGroupService _groupService;

    public ConfigurationApplicationCommandHandler(ILogger<ConfigurationApplicationCommandHandler> logger, IGroupService groupService) : base(
        "Configure", "Configure this bot for this server", logger) {
        _groupService = groupService;
    }

    public override Guid Id => Guid.Parse("C94327FC-2FBE-484B-B054-E1F88A02895C");
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

    private async Task<Result> ViewHandler(ApplicationCommandContext context) {
        var config = await _groupService.GetSettingsDictionary(context.Guild.ToGuildDto());

        var builder = new StringBuilder();
        foreach (var kvp in config) {
            builder.AppendLine($"**{kvp.Key}:** {kvp.Value}");
        }

        await context
            .CreateReplyBuilder()
            .WithEmbedFrom("Configuration", builder.ToString(), x => x.AddCommonProperties())
            .RespondAsync();
        
        return Result.Ok();
    }

    private async Task<Result> SetConfiguration(ApplicationCommandContext context) {
        var groupId = (int)context.SubCommandOptions.GetOptionValue<long>(WomGroupId);
        var verificationCode = context.SubCommandOptions.GetOptionValue<string>(WomVerificationCode);

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

        await context.CreateReplyBuilder(true)
            .WithEmbedFrom("Success", $"Group set to {decoratedGroup.Item.Name}", builder => builder
                .AddWiseOldMan(decoratedGroup)).RespondAsync();

        return Result.Ok();
    }

    protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
        builder
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(SetServerSubCommandName)
                .WithDescription("Set the basic WOM configuration for this bot")
                .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(WomGroupId, ApplicationCommandOptionType.Integer, "The ID of your wise old man group", true)
                    .AddOption(WomVerificationCode, ApplicationCommandOptionType.String, "The verification code of your group", true)
                    .AddOption(TimeZoneOption, ApplicationCommandOptionType.String, "Set the timezone", true, isAutocomplete:true)
            )
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(ViewConfigurationSubCommand)
                .WithDescription("View the current configuration")
                .WithType(ApplicationCommandOptionType.SubCommand)
            );
        return Task.FromResult(builder);
    }
}
