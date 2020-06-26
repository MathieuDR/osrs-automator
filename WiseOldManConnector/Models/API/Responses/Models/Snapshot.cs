using System;
using System.Collections.Generic;
using System.Linq;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class Snapshot {
        private Dictionary<MetricType, Metric> _metricDictionary = null;

        private List<MetricInfo> _metricInfoList = null;
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

        public List<MetricInfo> MetricInfoList {
            get { return _metricInfoList ??= (MetricDictionary.Select(x => x.Value.ToMetricInfo(x.Key)).ToList()); }
        }

        public Dictionary<MetricType, Metric> MetricDictionary {
            get {
                return _metricDictionary ??= new Dictionary<MetricType, Metric> {
                    {MetricType.Overall, Overall},
                    {MetricType.Attack, Attack},
                    {MetricType.Defence, Defence},
                    {MetricType.Strength, Strength},
                    {MetricType.Hitpoints, Hitpoints},
                    {MetricType.Ranged, Ranged},
                    {MetricType.Prayer, Prayer},
                    {MetricType.Magic, Magic},
                    {MetricType.Cooking, Cooking},
                    {MetricType.Woodcutting, Woodcutting},
                    {MetricType.Fletching, Fletching},
                    {MetricType.Fishing, Fishing},
                    {MetricType.Firemaking, Firemaking},
                    {MetricType.Crafting, Crafting},
                    {MetricType.Smithing, Smithing},
                    {MetricType.Mining, Mining},
                    {MetricType.Herblore, Herblore},
                    {MetricType.Agility, Agility},
                    {MetricType.Thieving, Thieving},
                    {MetricType.Slayer, Slayer},
                    {MetricType.Farming, Farming},
                    {MetricType.Runecrafting, Runecrafting},
                    {MetricType.Hunter, Hunter},
                    {MetricType.Construction, Construction}
                };
            }
        }
    }
}