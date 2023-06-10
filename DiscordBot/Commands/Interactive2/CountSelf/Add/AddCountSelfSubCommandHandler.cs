using System.Text;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data.Items;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.CountSelf.Add;

internal sealed  class AddCountSelfSubCommandHandler : ApplicationCommandHandlerBase<AddCountSelfSubCommandRequest> {
    private readonly ICounterService _countService;

    public AddCountSelfSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
        _countService = serviceProvider.GetRequiredService<ICounterService>();
    }
    protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
        await Context.DeferAsync();

        if (!await IsRequestChannel()) {
            _ = Context
                .CreateReplyBuilder(true)
                .WithEmbed(x => x.WithFailure("This channel is not configured for self counting request."))
                .FollowupAsync();
            
            return Result.Ok();
        }
        
        
        var (item, users, image) = await GetOptions();

        var r = ValidateOptions(item, users.Count, image);
        if (r.IsFailed) {
            return r;
        }

        await _countService.SelfCount(Context.GuildUser.ToGuildUserDto(), item, users.Select(x=>x.ToGuildUserDto()).ToArray(), image.Url);
        return SendEmbed(item, users, image);
    }

    private async Task<bool> IsRequestChannel() {
        var result = await _countService.CanSelfCountInChannel(Context.User.ToGuildUserDto(), Context.TextChannel.ToChannelDto());
        return result.IsSuccess && result.Value;
    }

    private Result ValidateOptions(Item item, int usersCount, Attachment image) {
        
            if (!Context.InGuild) {
                return Result.Fail("Command need to be executed in a guild");
            }

            if (item is null) {
                return Result.Fail("Item not found");
            }
        
            if (!item.Splittable && usersCount > 0) {
                return Result.Fail($"Item {item.Name} is not splittable, but you tried to split it with {usersCount} users.");
            }

            // check if the discord attachment is an image
            if (image != null) {
                var isImage = image.ContentType.StartsWith("image");
                if (!isImage) {
                    return Result.Fail("The attachment is not an image");
                }
            }

            return Result.Ok();
    }

    private Result SendEmbed(Item item, List<IUser> users, Attachment image) {
        var title = $"Requested points for {item.Name}";
        var descr = $"Requested {item.Value} points for {Context.User.DisplayName()}";
        
        _ = Context.CreateReplyBuilder()
            .WithEmbed(b => {
                b.WithSuccess(descr, title);

                if (users.Count > 0) {
                    b.AddField("Split with", string.Join(", ", users.Select(x => x.DisplayName())));
                }
                
                b.WithThumbnailUrl(image.Url);
            })
            .FollowupAsync();
        
        return Result.Ok();
    }

    private static string BuildDescription(int score, string reason, Dictionary<GuildUser, int> dataDict) {
        var descrBuilder = new StringBuilder();
        descrBuilder.Append("Added ");
        descrBuilder.Append(score);
        descrBuilder.Append(" points for '");
        descrBuilder.Append(reason);
        descrBuilder.AppendLine("'");
        descrBuilder.AppendLine("");

        foreach (var kvp in dataDict) {
            descrBuilder.Append(kvp.Key.Username);
            descrBuilder.Append(" point total: ");
            descrBuilder.AppendLine(kvp.Value.ToString());
        }

        return descrBuilder.ToString();
    }

 
    private async Task<(Item item, List<IUser> splits, Attachment image)> GetOptions() {
        var itemString = Context.SubCommandOptions.GetOptionValue<string>(AddCountSelfSubCommandDefinition.ItemOption);
        var usersString = Context.SubCommandOptions.GetOptionValue<string>(AddCountSelfSubCommandDefinition.SplitUsers);
        var image = Context.SubCommandOptions.GetOptionValue<Attachment>(AddCountSelfSubCommandDefinition.Image);
        var users = string.IsNullOrEmpty(usersString) ? Array.Empty<IUser>() : (await usersString.GetUsersFromString(Context)).users;

        var item = (await _countService.GetItemsForGuild(Context.Guild.ToGuildDto())).FirstOrDefault(x => x.Name == itemString);

        return (item, users.ToList(), image);
    }
}
