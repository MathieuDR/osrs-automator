namespace WiseOldManConnector.Models;

public class ConnectorResponse<T> : ConnectorResponseBase {
    internal ConnectorResponse(T data) : this(data, null) { }

    internal ConnectorResponse(T data, string message) : base(message) {
        Data = data;
    }

    public T Data { get; set; }
}
