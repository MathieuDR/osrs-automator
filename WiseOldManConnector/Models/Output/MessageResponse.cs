namespace WiseOldManConnector.Models.Output; 

public class MessageResponse : IBaseConnectorOutput {
    public string Message { get; set; }
    public bool IsError { get; set; }
}