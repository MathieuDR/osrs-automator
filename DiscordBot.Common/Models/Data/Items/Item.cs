namespace DiscordBot.Common.Models.Data.Items; 

public sealed record Item(string Name, List<string> Synonyms, int Value, bool Splittable) { }
