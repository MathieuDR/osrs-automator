namespace DiscordBot.Helpers.Builders;

public interface IResultInteractionReplyBuilder<TInteraction> : IBaseInteractionReplyBuilder<TInteraction> where TInteraction : SocketInteraction {
	IResultInteractionReplyBuilder<TInteraction> WithSuccessEmbed(Action<EmbedBuilder, Result> embedBuilderModification);
	IResultInteractionReplyBuilder<TInteraction> WithSuccessEmbed<T>(Action<EmbedBuilder, Result<T>> embedBuilderModification);
	IResultInteractionReplyBuilder<TInteraction> WithFailureEmbed(Action<EmbedBuilder, Result> embedBuilderModification);
}