using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Rest;

namespace Discord.Addons.Interactive.Paginator
{
    public class PaginatedAppearanceOptions
    {
        public static PaginatedAppearanceOptions Default = new PaginatedAppearanceOptions();

        public IEmote First = new Emoji("⏮");
        public IEmote Back = new Emoji("◀");
        public IEmote Next = new Emoji("▶");
        public IEmote Last = new Emoji("⏭");
        public IEmote Stop = new Emoji("⏹");
        public IEmote Jump = new Emoji("🔢");
        public IEmote Info = new Emoji("ℹ");

        public string FooterFormat = "Page {0}/{1}";
        public string InformationText = "This is a paginator. React with the respective icons to change page.";

        public JumpDisplayOptions JumpDisplayOptions = JumpDisplayOptions.WithManageMessages;
        public bool DisplayInformationIcon = true;

        public TimeSpan? Timeout = null;
        public TimeSpan InfoTimeout = TimeSpan.FromSeconds(30);

        public int FieldsPerPage = 6;

        public virtual async Task AddReactions(RestUserMessage message, SocketCommandContext context, int pages) {
            if (pages > 1) {
                await message.AddReactionAsync(First);
                await message.AddReactionAsync(Back);
                await message.AddReactionAsync(Next);
                await message.AddReactionAsync(Last);

                var manageMessages = (context.Channel is IGuildChannel guildChannel) && ((IGuildUser) context.User).GetPermissions(guildChannel).ManageMessages;

                if (JumpDisplayOptions == JumpDisplayOptions.Always
                    || (JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages))
                    await message.AddReactionAsync(Jump);
            }

            await message.AddReactionAsync(Stop);

            if (DisplayInformationIcon)
                await message.AddReactionAsync(Info);
        }
    }

    public enum JumpDisplayOptions
    {
        Never,
        WithManageMessages,
        Always
    }
}
