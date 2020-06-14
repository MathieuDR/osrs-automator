namespace WiseOldManConnector.Models.WiseOldMan {
    public class MessageResponse : WiseOldManObject {
        public string Message { get; set; }
        public bool IsError { get; set; }
    }
}