using System.Text;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Counting;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Count.Export;

public class ExportCountSubCommandHandler : ApplicationCommandHandlerBase<ExportCountSubCommandRequest> {
    private readonly ICounterService _counterService;
    const string CsvDelimiter = ",";

    public ExportCountSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
        _counterService = serviceProvider.GetRequiredService<ICounterService>();
    }

    protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
        var info = await _counterService.GetAllUserInfo(Context.Guild.ToGuildDto());
        var csv = CreateCsv(info);

        // string to stream
        using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream);
        await streamWriter.WriteAsync(csv);
        await streamWriter.FlushAsync();
        memoryStream.Position = 0;

        await Context.InnerContext.RespondWithFileAsync(memoryStream, GetFileName(), "count log for server", null, false, true, AllowedMentions.None, null, null,
            RequestOptions.Default);
        return Result.Ok();
    }

    private string CreateCsv(IEnumerable<UserCountInfo> infos) {
        var builder = new StringBuilder();
        WriteHeader(builder);
        foreach (var info in infos) {
            var (userName, discriminator, display) = ExtractUserInfo(info.DiscordId);
            foreach (var history in info.CountHistory) {
                WriteLine(builder, info, userName, discriminator, display, history);
            }
        }

        return builder.ToString();
    }

    private static void WriteLine(StringBuilder builder, UserCountInfo info, string userName, string discriminator, string display, Common.Models.Data.Counting.Count history) {
        builder.Append(info.DiscordId);
        builder.Append(CsvDelimiter);
        builder.Append(userName);
        builder.Append(CsvDelimiter);
        builder.Append(discriminator);
        builder.Append(CsvDelimiter);
        builder.Append(display);
        builder.Append(CsvDelimiter);
        builder.Append(history.Additive);
        builder.Append(CsvDelimiter);
        builder.Append(history.Reason);
        builder.Append(CsvDelimiter);
        builder.Append(history.RequestedBy);
        builder.Append(CsvDelimiter);
        builder.Append(history.RequestedDiscordTag);
        builder.Append(CsvDelimiter);
        builder.Append(history.RequestedOn.ToString("u"));
        builder.Append("\n");
    }

    private (string userName, string discriminator, string display) ExtractUserInfo(DiscordUserId id) {
        var user = Context.Guild.GetUser(id.UlongValue);
        var userName = user.Username;
        var discriminator = user.Discriminator;
        var display = user.DisplayName();
        return (userName, discriminator, display);
    }

    private static void WriteHeader(StringBuilder builder) {
        builder.Append("Id");
        builder.Append(CsvDelimiter);
        builder.Append("User Name");
        builder.Append(CsvDelimiter);
        builder.Append("User Discriminator");
        builder.Append(CsvDelimiter);
        builder.Append("Display Name");
        builder.Append(CsvDelimiter);
        builder.Append("Additive");
        builder.Append(CsvDelimiter);
        builder.Append("Reason");
        builder.Append(CsvDelimiter);
        builder.Append("RequestedById");
        builder.Append(CsvDelimiter);
        builder.Append("RequestedByTag");
        builder.Append(CsvDelimiter);
        builder.Append("RequestedOn");
        builder.Append("\n");
    }

    private static string GetFileName() => $"{DateTime.Now:yyyyMMdd-HHmm}_CountInfo.csv";
}