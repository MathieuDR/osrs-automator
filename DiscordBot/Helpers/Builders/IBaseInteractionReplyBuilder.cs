namespace DiscordBot.Helpers.Builders;

public interface IBaseInteractionReplyBuilder<TInteraction>  where TInteraction : SocketInteraction {
	Task RespondAsync(RequestOptions options = null);
	Task<RestFollowupMessage> FollowupAsync(RequestOptions options = null);
}