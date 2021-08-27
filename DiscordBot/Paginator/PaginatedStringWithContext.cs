namespace DiscordBot.Paginator {
    public class PaginatedStringWithContext<T> {
        public string StringValue { get; set; }
        public T Reference { get; set; }

        public override string ToString() {
            return StringValue;
        }
    }
}
