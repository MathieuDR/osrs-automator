using AutoMapper;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {
    [Name("JobTester")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    public class JobTestModule:BaseWaitMessageEmbeddedResponseModule {
        public JobTestModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration) : base(mapper, logger, messageConfiguration) { }
    }
}
