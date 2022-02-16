using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Dtos.Runescape;
using FluentResults;

namespace DiscordBot.Services.Interfaces;

public interface IAutomatedDropperService {
    public Task<Result<string>> RequestUrl(GuildUser user);
    public Task<Result> HandleDropRequest(ulong endpoint, RunescapeDrop drop, string base64Image);
}
