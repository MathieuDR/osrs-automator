using System.Threading.Tasks;
using DiscordBot.Common.Dtos.Runescape;
using FluentResults;

namespace DiscordBot.Services.Interfaces {
    public interface IAutomatedDropperService {
        public Task<Result> HandleDrop(RunescapeDrop drop, string base64Image);
    }
}