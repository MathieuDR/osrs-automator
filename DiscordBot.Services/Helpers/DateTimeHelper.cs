using DiscordBot.Common.Models.Data;
using TimeZoneConverter;

namespace DiscordBot.Services.Helpers; 

public static class DateTimeHelper {
    public static DateTimeOffset GetCorrectDateTimeOffset(this DateTime date, string timezoneKey) {
        var offset = timezoneKey.GetOffsetFromString();
        return new DateTimeOffset(date, offset);
    }

    private static TimeSpan GetOffsetFromString(this string timezoneKey) {
        if (string.IsNullOrEmpty(timezoneKey)) {
            timezoneKey = "UTC";
        }
        
        var tzInfo = TZConvert.GetTimeZoneInfo(timezoneKey);
        return tzInfo.GetUtcOffset(DateTime.UtcNow);
    }
    
}
