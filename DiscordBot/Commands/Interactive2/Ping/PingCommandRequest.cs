using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Ping; 

public class PingCommandRequest : ApplicationCommandRequestBase<PingRootCommandDefinition> {
    public PingCommandRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.None;
}

public class PingApplicationCommandHandler : ApplicationCommandHandlerBase<PingCommandRequest> {
    protected override async Task<Result> DoWork(IEnumerable<(string optionName, Type optionType, object? optionValue)> options) {
        await Context.RespondAsync($"At do work with {options.Count()} options");
        return Result.Ok();
    }
}

public class InsultCommandRequest : ApplicationCommandRequestBase<InsultSubCommandDefinition> {
    public InsultCommandRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanGuest;
}

public class InsultApplicationCommandHandler : ApplicationCommandHandlerBase<InsultCommandRequest> {
    protected override async Task<Result> DoWork(IEnumerable<(string optionName, Type optionType, object? optionValue)> options) {
        await Context.RespondAsync($"At insults do work with {options.Count()} options");
        return Result.Ok();
    }
}

