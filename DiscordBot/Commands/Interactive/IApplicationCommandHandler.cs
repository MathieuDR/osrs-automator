using System;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Models.Contexts;
using FluentResults;

namespace DiscordBot.Commands.Interactive; 

public interface IApplicationCommandHandler {
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public bool GlobalRegister { get; }
    Task<SlashCommandProperties> GetCommandProperties();
    Task<Result> HandleCommandAsync(ApplicationCommandContext context);
    Task<Result> HandleComponentAsync(MessageComponentContext context);
    public bool CanHandle(ApplicationCommandContext context);
    public bool CanHandle(MessageComponentContext context);
    Task<uint> GetCommandBuilderHash();
}