using AutoMapper;

namespace DiscordBot.Transformers.TypeConverters;

public class StringToEmbedConverter : ITypeConverter<string, Embed> {
    public Embed Convert(string source, Embed destination, ResolutionContext context) {
        var builder = new EmbedBuilder();
        builder.Title = "Success";
        builder.Description = source;
        builder.Color = Color.DarkPurple;
        builder.Timestamp = DateTimeOffset.Now;
        return builder.Build();
    }
}
