using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;

namespace DiscordBotFanatic.Models.WiseOldMan.Cleaned {
    public class RecordInfo : BaseInfo<Record> {
        public RecordInfo() { }

        public RecordInfo(Record record) : base(record, record.Metric) { }

        public bool IsEmptyRecord {
            get {
                return Info.Value <= 0;
            }
        }

    }
}