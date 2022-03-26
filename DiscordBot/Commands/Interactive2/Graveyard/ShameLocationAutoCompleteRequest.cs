using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Commands.Interactive2.Graveyard.Edit;
using DiscordBot.Commands.Interactive2.Graveyard.Leaderboard;
using DiscordBot.Commands.Interactive2.Graveyard.Shames;
using DiscordBot.Commands.Interactive2.Graveyard.ShameSubCommand;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard;

public class ShameLocationAutoCompleteRequest : AutoCompleteCommandRequestBase<ShameSubCommandDefinition>, IAutoCompleteCommandRequest<ShamesSubCommandDefinition>, IAutoCompleteCommandRequest<LeaderboardSubCommandDefinition>,  IAutoCompleteCommandRequest<EditShameSubcommandDefinition>{
	public ShameLocationAutoCompleteRequest(AutocompleteCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole=> AuthorizationRoles.ClanMember;
}
