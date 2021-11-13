using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Common.Models.Decorators;
using DiscordBot.Helpers.Builders;
using DiscordBot.Helpers.Extensions;
using DiscordBot.Models.Contexts;
using DiscordBot.Services.Interfaces;
using DiscordBot.Transformers;
using FluentResults;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Commands.Interactive; 

public class ConfigurationApplicationCommandHandler :  ApplicationCommandHandler {
    private readonly IGroupService _groupService;
    public ConfigurationApplicationCommandHandler(ILogger<ConfigurationApplicationCommandHandler> logger, IGroupService groupService) : base("Configure","Configure this bot for this server", logger) {
        _groupService = groupService;
    }
    public override Guid Id => Guid.Parse("C94327FC-2FBE-484B-B054-E1F88A02895C");
        
    private const string SetWomInfoCommandName = "wiseoldman";
    private const string WomGroupId = "group-id";
    private const string WomVerificationCode = "verification-code";
        
        
    public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
        var subCommand = context.Options.First().Key;

        var result = subCommand switch {
            SetWomInfoCommandName => await SetWomInformation(context),
            _ => Result.Fail("Did not find a correct handler")
        };

        return result;
    }

    private async Task<Result> SetWomInformation(ApplicationCommandContext context) {
        var groupId = (int)context.SubCommandOptions.GetOptionValue<long>(WomGroupId);
        var verificationCode = context.SubCommandOptions.GetOptionValue<string>(WomVerificationCode);

        if (groupId <= 0) {
            return Result.Fail("Group id must be higher then 0");
        }

        // Needs more verification
        if (string.IsNullOrEmpty(verificationCode)) {
            return Result.Fail("Group verification must be set");
        }

        ItemDecorator<Group> decoratedGroup;
        try {
            decoratedGroup = await _groupService.SetGroupForGuild(context.GuildUser.ToGuildUserDto(), groupId, verificationCode);
        } catch (Exception e) {
            return Result.Fail(new ExceptionalError(e));
        }
            
        await context.CreateReplyBuilder(true)
            .WithEmbedFrom("Success", $"Group set to {decoratedGroup.Item.Name}", builder => builder
                .AddWiseOldMan(decoratedGroup)).RespondAsync();
            
        return Result.Ok();
    }

    public override Task<Result> HandleComponentAsync(MessageComponentContext context) => throw new NotImplementedException();

    protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
        builder
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(SetWomInfoCommandName)
                .WithDescription("Set the channel for threshold messages")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(WomGroupId, ApplicationCommandOptionType.Integer, "The ID of your wise old man group", true)
                .AddOption(WomVerificationCode, ApplicationCommandOptionType.String, "The verification code of your group", true)
            );
        return Task.FromResult(builder);
    }
}