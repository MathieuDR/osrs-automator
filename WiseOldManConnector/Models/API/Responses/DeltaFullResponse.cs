namespace WiseOldManConnector.Models.API.Responses;

internal class DeltaFullResponse : BaseResponse {
    // TODO Rename
    public DeltaResponse Day { get; set; }
    public DeltaResponse Year { get; set; }
    public DeltaResponse Month { get; set; }
    public DeltaResponse Week { get; set; }
}