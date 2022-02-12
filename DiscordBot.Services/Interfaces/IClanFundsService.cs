using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using FluentResults;

namespace DiscordBot.Services.Interfaces; 

public interface IClanFundsService {
	Task<Result<IEnumerable<ClanFundEvent>>> GetClanFundEvents(Guild guild);
	Task<Result<ClanFunds>> GetClanFund(Guild guild);
	Task<Result> AddClanFund(Guild guild, ClanFundEvent clanFundEvent);
	
	Task<Result> Initialize(GuildUser guild, Channel trackingChannel, Channel donationChannel, long? currentFunds = null);
}
