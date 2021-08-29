using System.Collections.Generic;
using Discord;

namespace DiscordBot.Commands.Modules.DiscordCommandArguments {
    public class UserListWithImageArguments {
        public IEnumerable<IUser> Users { get; set; } = new List<IUser>();
        public string ImageUrl { get; set; }
    }
}
