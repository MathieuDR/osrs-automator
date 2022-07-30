using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace DiscordBot.Dashboard.Services;

public interface ICachedDiscordService {
	public Task<Result<IEnumerable<Channel>>> GetOrderedTextChannels(DiscordGuildId guildId);
	public Task<Result<Dictionary<Channel, IEnumerable<Channel>>>> GetNestedChannels(DiscordGuildId guildId);
	public Task<Result<Dictionary<Channel, IEnumerable<Channel>>>> GetNestedTextChannels(DiscordGuildId guildId);
	public Task<Result<IEnumerable<Guild>>> GetGuilds();
	public Result ResetCache();
	public Result ResetCache(DiscordGuildId guildId);
	public Task<Result<IEnumerable<GuildUser>>> GetUsers(DiscordGuildId guildId);
}

public class CachedCachedDiscordService : ICachedDiscordService {
	private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);
	private readonly IDiscordService _discordService;
	private readonly IMemoryCache _memoryCache;
	private readonly Dictionary<DiscordGuildId, CancellationTokenSource> _resetCacheTokens = new();


	public CachedCachedDiscordService(IDiscordService discordService, IMemoryCache memoryCache) {
		_discordService = discordService;
		_memoryCache = memoryCache;
	}


	public async Task<Result<IEnumerable<Channel>>> GetOrderedTextChannels(DiscordGuildId guildId) {
		var channels = await GetNestedTextChannels(guildId);
		if (channels.IsFailed) {
			return channels.ToResult();
		}
		
		var result = channels.Value.SelectMany(x => x.Value);
		return Result.Ok(result);
	}

	public async Task<Result<Dictionary<Channel, IEnumerable<Channel>>>> GetNestedChannels(DiscordGuildId guildId) => await GetFromCache(guildId, "nestedChannels", () => _discordService.GetNestedChannelsForGuild(guildId));

	public async Task<Result<Dictionary<Channel, IEnumerable<Channel>>>> GetNestedTextChannels(DiscordGuildId guildId) {
		var nestedChannels = await GetNestedChannels(guildId);
		if (!nestedChannels.IsSuccess) {
			return nestedChannels;
		}

		var result = nestedChannels.Value.Where(x => x.Value.Any(y => y.IsTextChannel))
			.ToDictionary(x => x.Key, x => x.Value);
		return Result.Ok(result);
	}

	public async Task<Result<IEnumerable<Guild>>> GetGuilds() => await GetFromCache("guilds", async () => await _discordService.GetGuilds());

	public async Task<Result<IEnumerable<GuildUser>>> GetUsers(DiscordGuildId guildId) => await GetFromCache(guildId, "users", async () => await _discordService.GetUsers(guildId));


	public Result ResetCache() {
		foreach (var kvp in _resetCacheTokens) {
			kvp.Value.Cancel();
			kvp.Value.Dispose();
			_resetCacheTokens.Remove(kvp.Key);
		}

		return Result.Ok();
	}

	public Result ResetCache(DiscordGuildId guildId) {
		if (!_resetCacheTokens.ContainsKey(guildId)) {
			return Result.Ok();
		}

		var token = _resetCacheTokens[guildId];

		token.Cancel();
		token.Dispose();
		_resetCacheTokens.Remove(guildId);
		return Result.Ok();
	}

	private async Task<Result<T>> GetFromCache<T>(DiscordGuildId guild, string key, Func<Task<Result<T>>> getFromService) {
		key = GetCacheKey(guild, key);
		var tokenSource = GetCancellationTokenSource(guild);
		return await GetFromCache(key, tokenSource, getFromService);
	}

	private async Task<Result<T>> GetFromCache<T>(string key, Func<Task<Result<T>>> getFromService) {
		var tokenSource = GetCancellationTokenSource(DiscordGuildId.Empty);
		return await GetFromCache(key, tokenSource, getFromService);
	}

	private CancellationTokenSource GetCancellationTokenSource(DiscordGuildId guild) {
		if (!_resetCacheTokens.ContainsKey(guild)) {
			_resetCacheTokens[guild] = new CancellationTokenSource();
		}

		return _resetCacheTokens[guild];
	}

	private async Task<Result<T>> GetFromCache<T>(string key, CancellationTokenSource tokenSource, Func<Task<Result<T>>> getFromService) {
		if (_memoryCache.TryGetValue(key, out T value)) {
			return Result.Ok(value);
		}

		var result = await getFromService();
		if (result.IsSuccess) {
			var options = new MemoryCacheEntryOptions {
				AbsoluteExpiration = DateTime.Now.Add(_cacheExpiration)
			};
			options.ExpirationTokens.Add(new CancellationChangeToken(tokenSource.Token));
			_memoryCache.Set(key, result.Value, options);
		}

		return result;
	}

	private string GetCacheKey(DiscordGuildId guildId) => $"{guildId}";
	private string GetCacheKey(DiscordGuildId guildId, string subject) => $"{guildId}-{subject}";
}
