using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Confirm;

internal sealed class ConfirmButtonHandler : MessageComponentHandlerBase<ConfirmButtonRequest> {
    private readonly IConfirmationService _confirmationService;
    
    public ConfirmButtonHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
        _confirmationService = serviceProvider.GetRequiredService<IConfirmationService>();
    }

    protected override Task<Result> DoWork(CancellationToken cancellationToken) {
        bool isConfirmed = Context.Parameters[0] == "confirmed";
        var result = Confirm(isConfirmed);
        if (result.IsFailed) {
            Context.Channel.SendMessageAsync("Something went wrong! Please try again later or contact an erkend IT'erke.");
        }

        var builder = Context.InnerContext.Message.Embeds.First().ToEmbedBuilder();
        // add empty field to reset the inline
        builder.AddField("\u200b", "\u200b");
        builder.AddField("Handled by", Context.GuildUser.DisplayName, true);
        builder.AddField("Handled at", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), true);
        builder.WithColor(isConfirmed ? new Color(0,255,102) : new Color(255, 51, 102));
        builder.AddField("Status", isConfirmed ? "Confirmed" : "Declined", true);

        Context.InnerContext.Message.ModifyAsync(x => {
            x.Embed = builder.Build();
            x.Components = null;
        });

        return Task.FromResult(Result.Ok());
    }
    
    

    private Result Confirm(bool isAccepted) {
        return _confirmationService.Confirm(isAccepted, Context.MessageId, Context.GuildUser.ToGuildUserDto());
    }
}