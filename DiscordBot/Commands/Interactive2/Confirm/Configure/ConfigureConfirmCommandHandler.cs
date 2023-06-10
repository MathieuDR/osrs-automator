using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Confirm.Configure; 

internal sealed class ConfigureConfirmCommandHandler : ApplicationCommandHandlerBase<ConfigureConfirmCommandRequest> {
    private readonly IConfirmationService _confirmationService;
    public ConfigureConfirmCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
        _confirmationService = serviceProvider.GetRequiredService<IConfirmationService>();
    }
    protected override Task<Result> DoWork(CancellationToken cancellationToken) {
        var channel = Context.SubCommandOptions.GetOptionValue<IChannel>(ConfigureConfirmCommandDefinition.ConfirmChannel);
        var result = _confirmationService.SetConfirmChannel(channel.ToChannelDto(), Context.User.ToGuildUserDto());

        if (result.IsFailed) {
            return Task.FromResult(result);
        }

        _ = Context.CreateReplyBuilder()
            .WithEmbed(x => x.WithSuccess("Confirmation configuration updated"))
            .RespondAsync();
        
        return Task.FromResult(Result.Ok());
    }
}
