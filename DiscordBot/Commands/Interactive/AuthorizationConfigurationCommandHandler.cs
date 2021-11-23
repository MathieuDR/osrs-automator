using System.Text;
using Common.Extensions;
using DiscordBot.Common.Models.Enums;
using WiseOldManConnector.Helpers;

namespace DiscordBot.Commands.Interactive;

public class AuthorizationConfigurationCommandHandler : ApplicationCommandHandler {
    private const string ViewSubCommand = "view";

    private const string AuthorizeSubCommand = "authorize";

    private const string MentionOption = "mentions";
    private const string AuthorizationLevel = "auth-level";
    private const string RemoveOption = "unauthorize";

    private readonly IAuthorizationService _authorizationService;

    private readonly AuthorizationRoles[] _editableRolesArray = {
        AuthorizationRoles.ClanAdmin,
        AuthorizationRoles.ClanModerator,
        AuthorizationRoles.ClanEventHost,
        AuthorizationRoles.ClanEventParticipant,
        AuthorizationRoles.ClanMember,
        AuthorizationRoles.ClanGuest
    };

    public AuthorizationConfigurationCommandHandler(ILogger<AuthorizationConfigurationCommandHandler> logger,
        IAuthorizationService authorizationService) : base("configuration-auth",
        "Configure your authorization user and roles", logger) {
        _authorizationService = authorizationService;
    }

    public override Guid Id => Guid.Parse("E63F5A17-7A7B-4046-AABA-2DBD70F90D0D");
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanAdmin;
    public override bool GlobalRegister => true;

    public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
        if (!context.InGuild) {
            return Result.Fail("You need to be in a guild to do this command!");
        }

        var subCommand = context.Options.First().Key;

        var result = subCommand switch {
            ViewSubCommand => await ViewHandler(context),
            AuthorizeSubCommand => await AuthorizeHandler(context),
            _ => Result.Fail("Did not find a correct handler")
        };

        return result;
    }

    private async Task<Result> ViewHandler(ApplicationCommandContext context) {
        var configResult = await _authorizationService.ViewConfig(context.Guild.ToGuildDto());
        var config = configResult.Value;

        var pages = new PageBuilder[_editableRolesArray.Length];

        for (var i = 0; i < _editableRolesArray.Length; i++) {
            var role = _editableRolesArray[i];

            var users = config.UserIds.Where(x => x.Value.HasFlag(role)).Select(x => x.Key.ToUser()).ToList();
            var roles = config.RoleIds.Where(x => x.Value.HasFlag(role)).Select(x => x.Key.ToRole()).ToList();

            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.Append("**Users:** ");
            descriptionBuilder.AppendLine(users.Any() ? string.Join(", ", users) : "None");

            descriptionBuilder.Append("**Roles:** ");
            descriptionBuilder.AppendLine(roles.Any() ? string.Join(", ", roles) : "None");

            var builder = context
                .CreatePageBuilder(descriptionBuilder.ToString())
                .WithTitle($"{role.ToFriendlyNameOrDefault()}");

            pages[i] = builder;
        }

        var paginator = context.GetBaseStaticPaginatorBuilder(pages);
        _ = context.SendPaginator(paginator.Build());
        return Result.Ok();
    }

    protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
        builder.AddOption(new SlashCommandOptionBuilder()
                .WithName(ViewSubCommand)
                .WithDescription("View the current authorization settings")
                .WithType(ApplicationCommandOptionType.SubCommand)
            )
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(AuthorizeSubCommand)
                .WithDescription("Add or remove an authorization")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(AuthorizationLevel)
                    .WithDescription("The authorization level to grant or take")
                    .WithRequired(true)
                    .AddEnumChoices(_editableRolesArray))
                .AddOption(MentionOption, ApplicationCommandOptionType.String, "The users and or roles to grant this too", true)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(RemoveOption)
                    .WithDescription("Remove the authorization")
                    .WithType(ApplicationCommandOptionType.Boolean)
                    .WithDefault(false)
                    .WithRequired(false)
                ));

        return Task.FromResult(builder);
    }


    #region authorize

    private async Task<Result> AuthorizeHandler(ApplicationCommandContext context) {
        var (role, mentions, unauthorize) = GetAddParameters(context);
        var (users, roles) = await GetUsersAndRoles(mentions, context);

        Logger.LogInformation("Users to add: {users}", users.Count);
        Logger.LogInformation("roles to add: {roles}", roles.Count);
        Logger.LogInformation("role: {role}", role.ToFriendlyNameOrDefault());
        Logger.LogInformation("Removing: {removing}", unauthorize);

        var result = Result.Ok();
        if (users.Any()) {
            var usersToAuthorize = users.Select(x => x.ToGuildUserDto());
            result = unauthorize
                ? await _authorizationService.RemoveUsersToRole(usersToAuthorize, role)
                : await _authorizationService.AddUsersToRole(usersToAuthorize, role);
        }

        if (!roles.Any()) {
            _ = context.CreateReplyBuilder().WithEmbedFrom("Success", "Authorization updated").RespondAsync();
            return result;
        }

        var rolesToAuthorize = roles.Select(x => x.ToRoleDto());
        var roleResult = unauthorize
            ? await _authorizationService.RemoveDiscordRolesToRole(rolesToAuthorize, role)
            : await _authorizationService.AddDiscordRolesToRole(rolesToAuthorize, role);

        var returningResult = Result.Merge(result, roleResult);

        if (returningResult.IsSuccess) {
            _ = context.CreateReplyBuilder().WithEmbedFrom("Success", "Authorization updated").RespondAsync();
        }

        return returningResult;
    }

    private async Task<(List<IGuildUser> users, List<IRole> roles)> GetUsersAndRoles(string mentions, ApplicationCommandContext context) {
        var @params = mentions.ToCollectionOfParameters().ToArray();
        var (users, roles, _) = await @params.GetDiscordUsersAndRolesListFromStrings(context);
        return (users.Select(x => x.Cast<IGuildUser>()).ToList(), roles.ToList());
    }

    private (AuthorizationRoles authorizationRole, string mentionString, bool unauthorize) GetAddParameters(ApplicationCommandContext context) {
        var authLevel = (int)context.SubCommandOptions.GetOptionValue<long>(AuthorizationLevel);
        var mentions = context.SubCommandOptions.GetOptionValue<string>(MentionOption);
        var unauthorize = context.SubCommandOptions.GetOptionValue<bool>(RemoveOption);

        var authRole = (AuthorizationRoles)authLevel;

        return (authRole, mentions, unauthorize);
    }

    #endregion
}
