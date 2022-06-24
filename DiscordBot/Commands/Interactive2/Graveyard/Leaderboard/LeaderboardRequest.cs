using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Leaderboard;

public class LeaderboardRequest : ApplicationCommandRequestBase<LeaderboardSubCommandDefinition> {
	public LeaderboardRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanMember;
}
