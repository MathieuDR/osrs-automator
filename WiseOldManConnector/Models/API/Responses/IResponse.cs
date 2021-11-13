namespace WiseOldManConnector.Models.API.Responses; 

internal interface IResponse { }

internal interface IMessageResponse : IResponse {
    public string Message { get; set; }
}