using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Count.Export;

public class ExportCountSubCommandHandler : ApplicationCommandHandlerBase<ExportCountSubCommandRequest> {
    private readonly ICounterService _counterService;

    public ExportCountSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
        _counterService = serviceProvider.GetRequiredService<ICounterService>();
    }

    protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
        var csv = await _counterService.GetCsvExport(Context.Guild.ToGuildDto());

        // string to stream
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);
        streamWriter.Write(csv);
        streamWriter.Flush();
        memoryStream.Position = 0;

        await Context.InnerContext.RespondWithFileAsync(memoryStream, GetFileName(), "count log for server", null, false, true, AllowedMentions.None, null, null,
            RequestOptions.Default);
        return Result.Ok();
    }

    private static string GetFileName() => $"{DateTime.Now:yyyyMMdd-HHmm}_CountInfo.csv";
}