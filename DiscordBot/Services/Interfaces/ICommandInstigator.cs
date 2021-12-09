namespace DiscordBot.Services.Interfaces;

public interface ICommandInstigator {
    Task<Result> ExecuteCommandAsync<T>(BaseInteractiveContext<T> context) where T : SocketInteraction;
    Task<Result> ExecuteCommandAsync(BaseInteractiveContext context);
}