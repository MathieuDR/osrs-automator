using System.Text;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data;

namespace DiscordBot.Commands.Interactive;

public class KillBotCommandHandler : ApplicationCommandHandler {
    private readonly LiteDbManager _manager;

    public KillBotCommandHandler(ILogger<KillBotCommandHandler> logger, LiteDbManager manager) : base("kill",
        "Kill me", logger) {
        _manager = manager;
    }

    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.BotAdmin;
    public override bool GlobalRegister => false;

    protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
        return Task.FromResult(builder);
    }

    public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
        Logger.LogInformation("Killing bot as requested by {0}", context.User.Username);

        await context.CreateReplyBuilder().WithEmbed(b => b.WithTitle("Killing bot")).RespondAsync();
        
        _manager.Dispose();
        Environment.Exit(0);
        return Result.Ok();
    }
}
