using System.Text;
using MathieuDR.Common.Extensions;

namespace DiscordBot.Helpers.Builders;

public class BaseInteractionReplyBuilder<TInteraction> : IInteractionReplyBuilder<TInteraction>, IResultInteractionReplyBuilder<TInteraction>
	where TInteraction : SocketInteraction {
	private readonly BaseInteractiveContext<TInteraction> _context;

	
	private Task _updateTask;

	public BaseInteractionReplyBuilder(BaseInteractiveContext<TInteraction> ctx) => _context = ctx;

	public string Content { get; private set; }
	public HashSet<Embed> Embeds { get; } = new();
	public bool IsTts { get; private set; }
	public bool IsEphemeral { get; private set; }
	public AllowedMentions AllowedMentions { get; private set; } = AllowedMentions.None;
	public Task UpdateOrNoopTask => _updateTask ?? Task.CompletedTask;
	public HashSet<ActionRowBuilder> ActionRows { get; } = new();
	public Result Result { get; private set; }

	public IInteractionReplyBuilder<TInteraction> WithContent(string content) {
		Content = content;
		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithEmbed(Action<EmbedBuilder> modifier) {
		Embeds.Add(_context.CreateEmbedBuilder().Apply(modifier).Build());
		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithEmbed(EmbedBuilder embedBuilder) {
		Embeds.Add(embedBuilder.Build());
		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithEmbeds(params EmbedBuilder[] embedBuilders) {
		foreach (var embed in embedBuilders) {
			Embeds.Add(embed.Build());
		}

		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithEmbeds(params Embed[] embeds) {
		foreach (var embed in embeds) {
			Embeds.Add(embed);
		}

		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithEmbedFrom(StringBuilder title, StringBuilder content) =>
		WithEmbedFrom(title.ToString(), content.ToString());

	public IInteractionReplyBuilder<TInteraction> WithEmbedFrom(string title, string content) {
		Embeds.Add(_context.CreateEmbedBuilder(title, content).Build());
		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithEmbedFrom(string title, string content, Action<EmbedBuilder> embedBuilder) {
		var builder = _context.CreateEmbedBuilder(title, content);
		embedBuilder(builder);
		Embeds.Add(builder.Build());
		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithTts(bool tts) {
		IsTts = tts;
		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithEphemeral(bool ephemeral = true) {
		IsEphemeral = ephemeral;
		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithAllowedMentions(AllowedMentions allowedMentions) {
		AllowedMentions = allowedMentions;
		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithComponentMessageUpdate(Action<MessageProperties> modifier) {
		if (_context is MessageComponentContext mctx) {
			_updateTask = mctx.InnerContext.Message.ModifyAsync(modifier);
		}

		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithComponent(ComponentBuilder builder) {
		WithActionRows(builder.ActionRows);
		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithActionRows(params ActionRowBuilder[] actionRows) {
		foreach (var row in actionRows) {
			ActionRows.Add(row);
		}

		return this;
	}

	public IInteractionReplyBuilder<TInteraction> WithActionRows(IEnumerable<ActionRowBuilder> actionRows) => WithActionRows(actionRows.ToArray());

	public IInteractionReplyBuilder<TInteraction> WithButtons(IEnumerable<ButtonBuilder> buttons) {
		return WithActionRows(buttons.Select(x => x.Build()).AsActionRow());
	}

	public IInteractionReplyBuilder<TInteraction> WithButtons(params ButtonBuilder[] buttons) {
		return WithActionRows(buttons.Select(x => x.Build()).AsActionRow());
	}

	public IInteractionReplyBuilder<TInteraction> WithSelectMenu(SelectMenuBuilder menu) {
		ActionRows.Add(new ActionRowBuilder().AddComponent(menu.Build()));
		return this;
	}

	public async Task RespondAsync(RequestOptions options = null) {
		CreateResultEmbed();
		await _context.RespondAsync(Content, Embeds.ToArray(), IsTts, IsEphemeral,
			AllowedMentions, options, new ComponentBuilder().AddActionRows(ActionRows).Build());
		await UpdateOrNoopTask;
	}

	public async Task<RestFollowupMessage> FollowupAsync(RequestOptions options = null) {
		var result = await _context.FollowupAsync(Content, Embeds.ToArray(), IsTts, IsEphemeral,
			AllowedMentions, options, new ComponentBuilder().AddActionRows(ActionRows).Build());
		await UpdateOrNoopTask;
		return result;
	}

	public IResultInteractionReplyBuilder<TInteraction> FromResult(Result result) {
		Result = result;
		return this;
	}
	
	public IResultInteractionReplyBuilder<TInteraction> FromResult<T>(Result<T> result) {
		return FromResult(result.ToResult());
	}

	public IResultInteractionReplyBuilder<TInteraction> WithSuccessEmbed(Action<EmbedBuilder, Result> embedBuilderModification) {
		if (Result is null || Result.IsFailed) {
			return this;
		} 
		
		var embedBuilder = _context.CreateEmbedBuilder();
		embedBuilder.WithSuccess("Everything went well!");
		embedBuilderModification?.Invoke(embedBuilder, Result);
		Embeds.Add(embedBuilder.Build());
		return this;
	}
	
	public IResultInteractionReplyBuilder<TInteraction> WithSuccessEmbed<T>(Action<EmbedBuilder, Result<T>> embedBuilderModification) {
		if (Result is null || Result.IsFailed) {
			return this;
		} 
		
		var embedBuilder = _context.CreateEmbedBuilder();
		embedBuilder.WithSuccess("Everything went well!");
		embedBuilderModification?.Invoke(embedBuilder, Result);
		Embeds.Add(embedBuilder.Build());
		return this;
	}
	
	public IResultInteractionReplyBuilder<TInteraction> WithFailureEmbed(Action<EmbedBuilder, Result> embedBuilderModification) {
		if (Result is null || Result.IsSuccess) {
			return this;
		} 
		
		var embedBuilder = _context.CreateEmbedBuilder();
		embedBuilder.WithFailure("Stuff went wrong!");
		embedBuilderModification?.Invoke(embedBuilder, Result);
		Embeds.Add(embedBuilder.Build());
		return this;
	}

	private void BeforeMessagingActions() {
		// Crease the result embed from results
		// this needs to happen 'later' because there might be extra actions.
		CreateResultEmbed();
	}

	private void CreateResultEmbed() {
		if (Embeds.Count != 0 || Result is null) {
			// if we have an embed or no result, return
			return;
		}

		var embed = _context.CreateEmbedBuilder();
		if (Result.IsSuccess) {
			embed.WithSuccess("Everything went well!");
		} else {
			embed.WithFailure("Stuff went wrong!");
			foreach (var error in Result.Errors) {
				embed.AddField(f => {
					f.Name = "Error: ";
					f.Value = error.Message;
				});
			}
		}

		Embeds.Add(embed.Build());
	}
}
