using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Requests {
    public class CompetitionRequest : BasePagedRequest {
        public string Title { get; set; }
        public MetricType? Metric { get; set; }
        public string Status { get; set; }
        public int? PlayerId { get; set; }

        public override void IsValid() {

            if (!Metric.HasValue && !PlayerId.HasValue && string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Status)) {
                ValidationDictionary.Add(nameof(CompetitionRequest), $"At least one parameter should be filled in.");
            }

            base.IsValid();
        }
    }
}