using DiscordBot.Common.Models.Data.Base;
using LiteDB;

namespace DiscordBot.Common.Models.Data.Items;

public sealed record Item : BaseRecord {
    public Item(string Name, List<string> Synonyms, int Value, int? SplitValue) {
        this.Name = Name;
        this.Synonyms = Synonyms;
        this.Value = Value;
        this.SplitValue = SplitValue;
        Id = ObjectId.NewObjectId();
    }

    public Item() { }

    public string Name { get; init; }
    public List<string> Synonyms { get; init; }
    public int Value { get; init; }
    public int? SplitValue { get; init; }

    [BsonIgnore]
    public bool Splittable => SplitValue.HasValue;

    public void Deconstruct(out string Name, out List<string> Synonyms, out int Value, out int? SplitValue, out bool Splittable) {
        Name = this.Name;
        Synonyms = this.Synonyms;
        Value = this.Value;
        SplitValue = this.SplitValue;
        Splittable = this.Splittable;
    }
}
