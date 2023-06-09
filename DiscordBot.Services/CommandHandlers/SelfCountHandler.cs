using DiscordBot.Common.Models.Commands;
using DiscordBot.Services.Interfaces;
using FluentResults;
using MediatR;

namespace DiscordBot.Services.CommandHandlers; 

internal sealed class SelfCountHandler : IRequestHandler<SelfCountConfirmCommand, Result> {
    private readonly ICounterService _counterService;

    public SelfCountHandler(ICounterService counterService) {
        _counterService = counterService;
    }
    
    public Task<Result> Handle(SelfCountConfirmCommand request, CancellationToken cancellationToken) {
        try {
            var reason = "Points requested for " + request.Item.Name;
            _ = _counterService.Count(new[] { request.RequestedBy }, request.Handler, request.Item.Value, reason);
            
            if (request.IsSplit) {
                var splitReason = "Split points for " + request.Item.Name + ", split from " + request.RequestedBy.Username;
                _ = _counterService.Count(request.Splits, request.Handler, request.Item.SplitValue!.Value, splitReason);
            }
            
            return Task.FromResult(Result.Ok());
        }catch (Exception e) {
            return Task.FromResult(Result.Fail(e.Message));
        }
        
    }
}
