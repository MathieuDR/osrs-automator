using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using FluentResults;
using MediatR;

namespace DiscordBot.Common.Models.Commands; 

public interface IConfirmCommand : IRequest<Result> {
    public String Title { get; }
    public String Description { get;}
    public string ImageUrl { get;}
    public EmbedFieldDto[] Fields { get;  }
    public GuildUser Handler { get; set; }
}
