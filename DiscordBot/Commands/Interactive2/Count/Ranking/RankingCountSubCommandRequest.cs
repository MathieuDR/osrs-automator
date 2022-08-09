using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Count.Ranking;

public class RankingCountSubCommandRequest : ApplicationCommandRequestBase<RankingCountSubcommandDefinition> {
    public RankingCountSubCommandRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanMember;
}