namespace DiscordBot.Common.Models.Decorators; 

public class ItemDecorator<T> {
    public ItemDecorator(T item, string title, string link) {
        Item = item;
        Title = title;
        Link = link;
    }

    public T Item { get; set; }
    public string Link { get; set; }
    public string Title { get; set; }
}