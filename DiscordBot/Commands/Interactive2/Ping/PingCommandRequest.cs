using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Ping; 

public class PingCommandRequest : ApplicationCommandRequestBase<PingRootCommandDefinition> {
    public PingCommandRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.None;
}

public class PingApplicationCommandHandler : ApplicationCommandHandlerBase<PingCommandRequest> {
    public override Task<Result> Handle(PingCommandRequest request, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
}