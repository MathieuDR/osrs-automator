using System;
using Discord.WebSocket;

namespace DiscordBot.Models.Exceptions {
    public class ResponseException : Exception {
        // ReSharper disable once NotAccessedField.Local
        private readonly SocketMessage _discordMessage;

        public ResponseException(string message, SocketMessage discordMessage) : base(message) {
            _discordMessage = discordMessage;
        }
    }
}