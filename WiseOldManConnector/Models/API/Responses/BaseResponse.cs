namespace WiseOldManConnector.Models.API.Responses;

internal abstract class BaseResponse : IMessageResponse {
    public string Message { get; set; }
}
