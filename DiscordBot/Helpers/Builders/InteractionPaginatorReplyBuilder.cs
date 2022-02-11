using System.Text;
using Fergun.Interactive.Pagination;

namespace DiscordBot.Helpers.Builders;

public class InteractionPaginatorReplyBuilder<TInteraction> where TInteraction : SocketInteraction {
	private readonly BaseInteractiveContext<TInteraction> _context;
	private Task _updateTask;
	private const int _maxPageLength = 4000;

	public InteractionPaginatorReplyBuilder(BaseInteractiveContext<TInteraction> ctx) => _context = ctx;

	public List<PageBuilder> Pages { get; } = new();

	public EmbedBuilder FrontPage { get; private set; }

	public Action<StaticPaginatorBuilder> BuilderModifications { get; private set; }

	public bool IsEphemeral { get; private set; }

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
	
		// prep functions
		var entryStrings = leaderboard.Entries.Select(x => x.ToStringWithPadding());
		//var (header, footer) = AdjustHeaderAndFooter(leaderboard.ToPaddedHeaderString(), null, true);
		var header = $"```{leaderboard.ToPaddedHeaderString()}";
		
		CreatePagesFromLines(entryStrings.ToArray(), 15, header, "```", x => x.WithTitle($"{leaderboard.Name} leaderboard"));

		return this;
	}


	public InteractionPaginatorReplyBuilder<TInteraction> WithLines(string[] lines, int linesPerPage = 15, string? header = null,
		string? footer = null, bool inCodeBlock = false, Action<EmbedBuilder>? modifications = null) {
		
		(header, footer) = AdjustHeaderAndFooter(header, footer, inCodeBlock);
		CreatePagesFromLines(lines, linesPerPage, header, footer, modifications);

		return this;
	}

	private void CreatePagesFromLines(string[] lines, int linesPerPage, string? header, string? footer, Action<EmbedBuilder>? modifications) {
		int footerSize = footer?.Length ?? 0;

		var currentPage = new StringBuilder();
		
		if (footerSize + (header?.Length ?? 0) >= _maxPageLength) {
			throw new Exception("Header and footer combined are too long");
		}
		
		// append header at start
		if (header != null) {
			currentPage.AppendLine(header);
		}
		
		for (var i = 0; i < lines.Length; i++) {
			var line = lines[i];

			// check if it's the end of the page
			if (currentPage.Length + line.Length + footerSize + 1 > _maxPageLength || (i + 1) % linesPerPage == 0) {
				// if it is, add footer and create new page
				if (footer != null) {
					currentPage.AppendLine(footer);
				}

				var builder = _context.CreateEmbedBuilder("", currentPage.ToString());
				if (modifications != null) {
					modifications(builder);
				}

				Pages.Add(PageBuilder.FromEmbedBuilder(builder));

				currentPage = new StringBuilder();
				if (header != null) {
					currentPage.AppendLine(header);
				}
			}

			currentPage.AppendLine(line);
		}

		// Handle leftover
		if (currentPage.Length > 0) {
			// add footer
			if (footer != null) {
				currentPage.AppendLine(footer);
			}

			// create page
			var builder = _context.CreateEmbedBuilder("", currentPage.ToString());
			if (modifications != null) {
				modifications(builder);
			}

			Pages.Add(PageBuilder.FromEmbedBuilder(builder));
		}
	}

	private static (string? header, string? footer) AdjustHeaderAndFooter(string? header, string? footer, bool inCodeBlock) {
		if (inCodeBlock) {
			header = $"{header}{Environment.NewLine}```{Environment.NewLine}";
			footer = $"{Environment.NewLine}```{Environment.NewLine}{footer}";
		}

		return (header, footer);
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

	public InteractionPaginatorReplyBuilder<TInteraction> WithEphemeral(bool ephemeral) {
		IsEphemeral = ephemeral;
		return this;
	}
}
