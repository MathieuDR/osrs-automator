using AutoMapper;
using Discord;
using Discord.Commands;
using DiscordBot.Common.Configuration;
using DiscordBot.Services.interfaces;

namespace DiscordBot.Modules {
    [Name("JobTester")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    public class JobTestModule:BaseWaitMessageEmbeddedResponseModule {
        public JobTestModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration) : base(mapper, logger, messageConfiguration) { }
    }
}
