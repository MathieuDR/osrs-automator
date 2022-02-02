namespace WiseOldManConnector.Models.API.Responses;

internal interface IMessageResponse : IResponse {
	public string Message { get; set; }
}