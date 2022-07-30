using DiscordBot.Common.Dtos.Discord;

namespace DiscordBot.Dashboard.Models; 

public class ExtendedChannel {
    public ExtendedChannel(Channel channel, bool isDisabled, bool isGray) {
        Channel = channel;
        IsDisabled = isDisabled;
        IsGray = isGray;
    }
    public Channel Channel { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsGray { get; set; }
}
