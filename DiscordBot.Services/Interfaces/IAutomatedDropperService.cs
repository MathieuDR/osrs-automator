using System;
using System.Threading.Tasks;
using DiscordBot.Common.Dtos.Runescape;
using FluentResults;

namespace DiscordBot.Services.Interfaces; 

public interface IAutomatedDropperService {
    public Task<Result> HandleDropRequest(Guid endpoint, RunescapeDrop drop, string base64Image);
}