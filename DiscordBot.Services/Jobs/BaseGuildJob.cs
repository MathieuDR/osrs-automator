using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Jobs; 

public abstract class BaseGuildJob : BaseJob {
    protected BaseGuildJob(ILogger logger, IDiscordService discordService) : base(logger) {
        DiscordService = discordService;
    }

    protected IDiscordService DiscordService { get; }

    protected override async Task<Result> DoWork() {
        var guildsResult = await DiscordService.GetAllGuilds();
        if (guildsResult.IsFailed) {
            return guildsResult.ToResult();
        }

        var guilds = guildsResult.Value.ToArray();
        var tasks = new Task<Result>[guilds.Length];
        for (var i = 0; i < guilds.Length; i++) {
            var guild = guilds[i];
            var scopeProps = new Dictionary<string, object> {
                {"guild", guild}
            };

            using (Logger.BeginScope(scopeProps)) {
                Logger.LogInformation("Starting guild work");
                tasks[i] = DoWorkForGuild(guild);
            }
        }

        ResultBase[] results = await Task.WhenAll(tasks);
        return Result.Merge(results);
    }

    protected abstract Task<Result> DoWorkForGuild(Guild guild);
}