namespace DiscordBot.Helpers.Builders;

public interface IResultInteractionReplyBuilder<TInteraction> : IBaseInteractionReplyBuilder<TInteraction> where TInteraction : SocketInteraction {
	IResultInteractionReplyBuilder<TInteraction> WithSuccessEmbed(Action<EmbedBuilder, Result> embedBuilder);
	IResultInteractionReplyBuilder<TInteraction> WithFailureEmbed(Action<EmbedBuilder, Result> embedBuilder);
}