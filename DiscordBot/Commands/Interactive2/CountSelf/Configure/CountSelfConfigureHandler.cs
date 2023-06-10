using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.CountSelf.Configure; 

internal sealed class CountSelfConfigureHandler : ApplicationCommandHandlerBase<CountSelfConfigureRequest> {
    
    private readonly ICounterService _counterService;
    
    public CountSelfConfigureHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
        _counterService = serviceProvider.GetRequiredService<ICounterService>();
    }
    protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
        var (channel, jsonAttachment) = ParseOptions();
        var reply = Context.CreateReplyBuilder(true);

        var jsonResult = await HandleJsonAttachmentResult(jsonAttachment, reply);
        if (jsonResult.IsFailed) {
            return jsonResult;
        }
        
        var channelResult = await HandleChannelOptionResult(channel, reply);
        if (channelResult.IsFailed) {
            return channelResult;
        }

        if (!reply.HasEmbeds) {
            return Result.Fail("No json or channel provided");
        }

        _ = reply.RespondAsync();
        return Result.Ok();
    }

    private async Task<Result> HandleChannelOptionResult(IChannel channel, IInteractionReplyBuilder<SocketSlashCommand> reply) {
        if (channel is null) {
            return Result.Ok();
        }

        var result = await _counterService.SetRequestChannel(Context.GuildUser.ToGuildUserDto(), channel.ToChannelDto());
        if (result.IsFailed) {
            return result;
        }

        reply.WithEmbed(x => x.WithSuccess("Channel set."));
        return Result.Ok();
    }

    private async Task<Result> HandleJsonAttachmentResult(Attachment jsonAttachment, IInteractionReplyBuilder<SocketSlashCommand> reply) {
        if (jsonAttachment is null) {
            return Result.Ok();
        }

        var json = await GetJsonFromAttachment(jsonAttachment);
        if (string.IsNullOrWhiteSpace(json)) {
            return Result.Ok();
        }

        var result = await _counterService.SaveItemsFromJson(Context.GuildUser.ToGuildUserDto(), json);
        if (result.IsFailed) {
            return result;
        }

        reply.WithEmbed(x => x.WithSuccess("Items saved from json."));

        return Result.Ok();
    }

    private (IChannel channel, Attachment jsonAttachment) ParseOptions() {
        var channel = Context.Options.GetOptionValue<IChannel>(CountSelfConfigureCommandDefinition.Channel);
        var jsonFile = Context.Options.GetOptionValue<Attachment>(CountSelfConfigureCommandDefinition.Json);
         
        
        return (channel, jsonFile);
    }
    
    private async Task<string> GetJsonFromAttachment(Attachment attachment) {
        // check if it's json file
        if(attachment.ContentType != "application/json") {
            return null;
        }

        return await attachment.DownloadFileAsString();

    }
}
