using DiscordBot.Data;
using FluentResults;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DiscordBot.Services.Jobs;

public class MemoryReportJob : BaseJob {
    private readonly LiteDbManager _manager;
    public MemoryReportJob(ILogger<MemoryReportJob> logger, LiteDbManager manager) : base(logger) => _manager = manager;
    protected override Task<Result> DoWork() {
        using var proc = System.Diagnostics.Process.GetCurrentProcess();
        Logger.LogInformation("Memory report: workingSet={WorkingSetMb} MB, gcHeap={GcHeapMb} MB, handles={Handles}, openDatabases={OpenDatabases}",
            Environment.WorkingSet / 1_048_576, GC.GetTotalMemory(false) / 1_048_576, proc.HandleCount, _manager.OpenDatabaseCount);
        return Task.FromResult(Result.Ok());
    }
}
