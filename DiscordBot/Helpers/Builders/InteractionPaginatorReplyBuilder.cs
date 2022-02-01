using System.Text;
using Common.Extensions;
using Fergun.Interactive.Pagination;

namespace DiscordBot.Helpers.Builders;

public class InteractionPaginatorReplyBuilder<TInteraction> where TInteraction : SocketInteraction {
	private readonly BaseInteractiveContext<TInteraction> _context;
	private Task _updateTask;

	public List<PageBuilder> Pages { get; private set; } = new List<PageBuilder>();
    
	public EmbedBuilder FrontPage { get; private set; } 
    
	public Action<StaticPaginatorBuilder> BuilderModifications { get; private set; }
    
	public InteractionPaginatorReplyBuilder(BaseInteractiveContext<TInteraction> ctx) {
		_context = ctx;
	}

	public InteractionPaginatorReplyBuilder<TInteraction> WithFrontPage(EmbedBuilder builder) {
		FrontPage = builder;
		return this;
	}
    
	public InteractionPaginatorReplyBuilder<TInteraction> WithPaginatorModifications(Action<StaticPaginatorBuilder> modifications) {
		BuilderModifications = modifications;
		return this;
	}
    
	public InteractionPaginatorReplyBuilder<TInteraction> WithLeaderboard<T>(Models.Leaderboard<T> leaderboard) {
		// Order leaderboard by rank
		leaderboard.Entries = leaderboard.Entries.OrderBy(x => x.Rank).ToList();

		var headerString = leaderboard.ToPaddedHeaderString();
		var entryStrings = leaderboard.Entries.Select(x => x.ToStringWithPadding());
		var chunkedStrings = System.Linq.Enumerable.Chunk(entryStrings, 15);

		foreach (var strings in chunkedStrings) {
			var descriptionBuilder = new StringBuilder();

			descriptionBuilder.AppendLine("```");
			descriptionBuilder.AppendLine(headerString);
			strings.ForEach(x => descriptionBuilder.AppendLine(x));
			descriptionBuilder.AppendLine("```");
			
			var builder = _context.CreateEmbedBuilder($"{leaderboard.Name} leaderboard", descriptionBuilder.ToString());

			var pageBuilder = PageBuilder.FromEmbedBuilder(builder);
			Pages.Add(pageBuilder);
		}

		return this;
	}
    
	public Task RespondAsync() {
		// insert item as first item in list
		if (FrontPage is not null) {
			Pages.Insert(0, PageBuilder.FromEmbedBuilder(FrontPage));
		}

		var paginator = _context.GetBaseStaticPaginatorBuilder(Pages);
		
		BuilderModifications?.Invoke(paginator);

		_ = _context.SendPaginator(paginator.Build(), ephemeral: IsEphemeral);

		return Task.CompletedTask;
	}

	public bool IsEphemeral { get; private set; } = false;

	public InteractionPaginatorReplyBuilder<TInteraction> WithEphemeral(bool ephemeral)  {
		IsEphemeral = ephemeral;
		return this;
	}
}
