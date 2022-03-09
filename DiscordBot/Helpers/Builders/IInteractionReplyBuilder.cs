using System.Text;

namespace DiscordBot.Helpers.Builders;

public interface IInteractionReplyBuilder<TInteraction> : IBaseInteractionReplyBuilder<TInteraction> where TInteraction : SocketInteraction {
	IInteractionReplyBuilder<TInteraction> WithContent(string content);
	IInteractionReplyBuilder<TInteraction> WithEmbed(Action<EmbedBuilder> modifier);
	IInteractionReplyBuilder<TInteraction> WithEmbed(EmbedBuilder embedBuilder);
	IInteractionReplyBuilder<TInteraction> WithEmbeds(params EmbedBuilder[] embedBuilders);
	IInteractionReplyBuilder<TInteraction> WithEmbeds(params Embed[] embeds);
	IInteractionReplyBuilder<TInteraction> WithEmbedFrom(StringBuilder title, StringBuilder content);
	IInteractionReplyBuilder<TInteraction> WithEmbedFrom(string title, string content);
	IInteractionReplyBuilder<TInteraction> WithEmbedFrom(string title, string content, Action<EmbedBuilder> embedBuilder);
	IInteractionReplyBuilder<TInteraction> WithTts(bool tts);
	IInteractionReplyBuilder<TInteraction> WithEphemeral(bool ephemeral = true);
	IInteractionReplyBuilder<TInteraction> WithAllowedMentions(AllowedMentions allowedMentions);
	IInteractionReplyBuilder<TInteraction> WithComponentMessageUpdate(Action<MessageProperties> modifier);
	IInteractionReplyBuilder<TInteraction> WithComponent(ComponentBuilder builder);
	IInteractionReplyBuilder<TInteraction> WithActionRows(params ActionRowBuilder[] actionRows);
	IInteractionReplyBuilder<TInteraction> WithActionRows(IEnumerable<ActionRowBuilder> actionRows);
	IInteractionReplyBuilder<TInteraction> WithButtons(IEnumerable<ButtonBuilder> buttons);
	IInteractionReplyBuilder<TInteraction> WithButtons(params ButtonBuilder[] buttons);
	IInteractionReplyBuilder<TInteraction> WithSelectMenu(SelectMenuBuilder menu);
	IResultInteractionReplyBuilder<TInteraction> FromResult(Result result);
}