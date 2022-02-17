namespace DiscordBot.Common.Models.Data.ClanFunds;

public record ClanFundEvent { 
	public ClanFundEvent() { } // necessary for litedb
	
	public ClanFundEvent(DiscordUserId playerId, DiscordUserId creatorId, string reason, string playerName, long amount, ClanFundEventType eventType) {
		PlayerId = playerId;
		CreatorId = creatorId;
		Reason = reason;
		PlayerName = playerName;
		Amount = amount;
		EventType = eventType;
	}

	// guid id of event
	public Guid Id { get; init; } = Guid.NewGuid();

	// Id of player
	public DiscordUserId PlayerId { get; init; }
	
	// id of creator
	public DiscordUserId CreatorId { get; init; }
	
	// reason of event
	public string Reason { get; init; }

	// current player name
	public string PlayerName { get; init; }

	// Amount of money
	public long Amount { get; init; } = 0;

	// Type of event
	public ClanFundEventType EventType { get; init; }

	// datetime offset of event
	public DateTimeOffset EventDate { get; init; } = DateTimeOffset.UtcNow;
}