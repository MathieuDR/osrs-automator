using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses;

internal class DeltaResponse : BaseResponse {
	// TODO Rename
	public string Period { get; set; }

	//public DateTime UpdatedAt { get; set; }
	public DateTime StartsAt { get; set; }

	public DateTime EndsAt { get; set; }
	//public string Interval { get; set; }

	[JsonProperty("data")]
	public DeltaMetrics Metrics { get; set; }
}