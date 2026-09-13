namespace DiscordBot.Services.Models;

public record TemplateCatalogEntry(string Kind, int? TierMinDays, string Text, int UsageCount);
