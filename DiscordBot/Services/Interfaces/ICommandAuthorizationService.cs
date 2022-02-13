using DiscordBot.Commands.Interactive;
using DiscordBot.Commands.Interactive2.Base.Requests;

namespace DiscordBot.Services.Interfaces;

public interface ICommandAuthorizationService {
	public ValueTask<bool> IsAuthorized<T>(ICommandRequest<BaseInteractiveContext<T>> request, BaseInteractiveContext<T> context) where T : SocketInteraction;

	[Obsolete("Use IsAuthorized<T> instead")]
	public ValueTask<bool> IsAuthorized<T>(BaseInteractiveContext<T> context, IApplicationCommandHandler applicationCommand)
		where T : SocketInteraction;
}
