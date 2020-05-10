using System;
using System.Collections.Generic;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses.Models {
    public class Snapshot {
        public DateTime CreatedAt { get; set; }
        public object ImportedAt { get; set; }
        public Metric Overall { get; set; }
        public Metric Attack { get; set; }
        public Metric Defence { get; set; }
        public Metric Strength { get; set; }
        public Metric Hitpoints { get; set; }
        public Metric Ranged { get; set; }
        public Metric Prayer { get; set; }
        public Metric Magic { get; set; }
        public Metric Cooking { get; set; }
        public Metric Woodcutting { get; set; }
        public Metric Fletching { get; set; }
        public Metric Fishing { get; set; }
        public Metric Firemaking { get; set; }
        public Metric Crafting { get; set; }
        public Metric Smithing { get; set; }
        public Metric Mining { get; set; }
        public Metric Herblore { get; set; }
        public Metric Agility { get; set; }
        public Metric Thieving { get; set; }
        public Metric Slayer { get; set; }
        public Metric Farming { get; set; }
        public Metric Runecrafting { get; set; }
        public Metric Hunter { get; set; }
        public Metric Construction { get; set; }

        public Dictionary<string, Metric> MetricDictionary {
            get {
                var result = new Dictionary<string, Metric>();

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