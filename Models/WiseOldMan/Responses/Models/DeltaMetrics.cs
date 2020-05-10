using System.Collections.Generic;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses.Models {
    public class DeltaMetrics {
        public DeltaMetric Overall { get; set; }
        public DeltaMetric Attack { get; set; }
        public DeltaMetric Defence { get; set; }
        public DeltaMetric Strength { get; set; }
        public DeltaMetric Hitpoints { get; set; }
        public DeltaMetric Ranged { get; set; }
        public DeltaMetric Prayer { get; set; }
        public DeltaMetric Magic { get; set; }
        public DeltaMetric Cooking { get; set; }
        public DeltaMetric Woodcutting { get; set; }
        public DeltaMetric Fletching { get; set; }
        public DeltaMetric Fishing { get; set; }
        public DeltaMetric Firemaking { get; set; }
        public DeltaMetric Crafting { get; set; }
        public DeltaMetric Smithing { get; set; }
        public DeltaMetric Mining { get; set; }
        public DeltaMetric Herblore { get; set; }
        public DeltaMetric Agility { get; set; }
        public DeltaMetric Thieving { get; set; }
        public DeltaMetric Slayer { get; set; }
        public DeltaMetric Farming { get; set; }
        public DeltaMetric Runecrafting { get; set; }
        public DeltaMetric Hunter { get; set; }
        public DeltaMetric Construction { get; set; }

        public Dictionary<string, DeltaMetric> DeltaMetricDictionary {
            get {
                var result = new Dictionary<string, DeltaMetric>();

                result.Add(nameof(Overall), Overall);
                result.Add(nameof(Attack), Attack);
                result.Add(nameof(Defence), Defence);
                result.Add(nameof(Strength), Strength);
                result.Add(nameof(Hitpoints), Hitpoints);
                result.Add(nameof(Ranged), Ranged);
                result.Add(nameof(Prayer), Prayer);
                result.Add(nameof(Magic), Magic);
                result.Add(nameof(Cooking), Cooking);
                result.Add(nameof(Woodcutting), Woodcutting);
                result.Add(nameof(Fletching), Fletching);
                result.Add(nameof(Fishing), Fishing);
                result.Add(nameof(Firemaking), Firemaking);
                result.Add(nameof(Crafting), Crafting);
                result.Add(nameof(Smithing), Smithing);
                result.Add(nameof(Mining), Mining);
                result.Add(nameof(Herblore), Herblore);
                result.Add(nameof(Agility), Agility);
                result.Add(nameof(Thieving), Thieving);
                result.Add(nameof(Slayer), Slayer);
                result.Add(nameof(Farming), Farming);
                result.Add(nameof(Runecrafting), Runecrafting);
                result.Add(nameof(Hunter), Hunter);
                result.Add(nameof(Construction), Construction);

                return result;
            }
        }
    }
}