using DiscordBot.Commands.Interactive2.Count.Add;
using DiscordBot.Commands.Interactive2.CountSelf.Add;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.CountSelf;

internal sealed  class CountSelfItemAutoCompleteRequest : AutoCompleteCommandRequestBase<AddCountSelfSubCommandDefinition>{
    public CountSelfItemAutoCompleteRequest(AutocompleteCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole=> AuthorizationRoles.ClanMember;
}

