namespace WiseOldManConnector.Models {
    internal abstract class ConnectorResponseBase {

        internal ConnectorResponseBase(string message) {
            WiseOldManMessage = message;
        }
        
        public string WiseOldManMessage { get; set; }

        public bool HasMessage => string.IsNullOrEmpty(WiseOldManMessage);
    }
}