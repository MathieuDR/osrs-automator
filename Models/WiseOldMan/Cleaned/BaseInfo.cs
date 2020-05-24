using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;

namespace DiscordBotFanatic.Models.WiseOldMan.Cleaned {
    public interface IBaseInfo {
        public MetricType Type { get; set; }
    }

    public abstract class BaseInfo<T> : IBaseInfo {
        protected BaseInfo() { }

        protected BaseInfo(string type) {
            Type = type.ToMetricType();
        }

        protected BaseInfo(T info, MetricType type) {
            Info = info;
            Type = type;
        }

        protected BaseInfo(T info, string type) : this(info, type.ToMetricType()) { }

        public T Info{ get; set; }

       
        public MetricType Type { get; set; }
    }
}