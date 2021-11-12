using System;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot.Models.Contexts {
    public class SerilogCommandContext {
        public SerilogCommandContext(SocketCommandContext context) {
            Message = context.Message.Content;
            UserName = context.User.Username;
            Discriminator = context.User.Discriminator;
            IsPrivate = context.IsPrivate;
            if (!IsPrivate) {
                Guild = context.Guild.Name;
                Channel = context.Channel.Name;
            }
        }

        public string Message { get; set; }
        public string User => $"{UserName}#{Discriminator}";
        public string UserName { get; set; }
        public string Discriminator { get; set; }
        public string Channel { get; set; }
        public string Guild { get; set; }
        public bool IsPrivate { get; set; }

        public string MessageLocation => IsPrivate ? "Direct Message" : $"{Channel} ({Guild})";

        public override string ToString() {
            return $"{User} in {MessageLocation}: \"{Message}\"";
        }
    }
}
