using DiscordBot.Common.Models.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Confirm; 

internal sealed class ConfirmRootCommandDefinition : RootCommandDefinitionBase{
    public ConfirmRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider, subCommandDefinitions) { }
    public override string Name => "confirm";
    public override string Description => "Confirm button stuff";
}

internal sealed class ConfirmButtonRequest : MessageComponentRequestBase<ConfirmRootCommandDefinition> {
    public ConfirmButtonRequest(MessageComponentContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanModerator;
}

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
        builder.AddField("Handled by", Context.GuildUser.DisplayName);
        builder.AddField("Handled at", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        builder.WithColor(isConfirmed ? new Color(0,255,102) : new Color(255, 51, 102));
        builder.AddField("Status", isConfirmed ? "Confirmed" : "Declined");

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