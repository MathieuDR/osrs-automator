using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository.Migrations;

public sealed class MigrationToBetterCountModels : BaseMigration {
    public MigrationToBetterCountModels(ILogger<MigrationToBetterCountModels> logger) : base(logger) { }
    public override int Version => 1;

    public override void DoUp(LiteDatabase database) {
        var collection = database.GetCollection("guildConfig");

        foreach (var row in collection.FindAll()) {
            var countConfig = row["CountConfig"].AsDocument;
            if (countConfig == null) {
                continue;
            }

            countConfig["Thresholds"] = countConfig["_tresholds"];
            countConfig.Remove("_tresholds");

            var thresholds = countConfig["Thresholds"].AsArray;
            foreach (var threshold in thresholds) {
                var thresholdDoc = threshold.AsDocument;
                thresholdDoc["Threshold"] = thresholdDoc["Treshold"];
            }

            collection.Update(row);
        }
    }

    public override void DoDown(LiteDatabase database) {
        // This is untested!
        var collection = database.GetCollection("guildConfig");

        foreach (var row in collection.FindAll()) {
            var countConfig = row["CountConfig"].AsDocument;
            if (countConfig == null) {
                continue;
            }

            // Update Threshold fields
            countConfig["_tresholds"] = countConfig["Thresholds"];
            countConfig.Remove("Thresholds");

            var thresholds = countConfig["_tresholds"].AsArray;
            foreach (var threshold in thresholds) {
                var thresholdDoc = threshold.AsDocument;
                thresholdDoc["Treshold"] = thresholdDoc["Threshold"];
            }

            collection.Update(row);
        }
    }
}
