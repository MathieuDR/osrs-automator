using System.Text;

namespace DiscordBot.Helpers.Extensions; 

internal static class AttachmentHelper {
    public static async Task<MemoryStream> DownloadFile(this Attachment attachment) {
        var stream = new MemoryStream();
        var client = new HttpClient();
        var response = await client.GetAsync(attachment.Url);
        await response.Content.CopyToAsync(stream);
        stream.Position = 0;
        return stream;
    }
    
    // Now download an attachment to utf8 encoded string
    public static async Task<string> DownloadFileAsString(this Attachment attachment) {
        var stream = await DownloadFile(attachment);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
