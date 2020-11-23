using System;
using Discord.WebSocket;

namespace DiscordBotFanatic.Models.Exceptions {
    public class ResponseException : Exception {
        public ResponseException(string message, SocketMessage discordMessage) : base(message) { }
    }
}