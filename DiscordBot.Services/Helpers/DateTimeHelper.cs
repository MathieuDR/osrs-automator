using DiscordBot.Common.Models.Data;
using TimeZoneConverter;

namespace DiscordBot.Services.Helpers; 

public static class DateTimeHelper {
    public static DateTimeOffset GetCorrectDateTimeOffset(this DateTime date, string timezoneKey) {
        var tzInfo = TZConvert.GetTimeZoneInfo(timezoneKey);
        var offset = tzInfo.GetUtcOffset(date);
        return new DateTimeOffset(date, offset);
    }
}
