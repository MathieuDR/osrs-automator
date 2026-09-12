namespace DiscordBot.Services.Helpers;

/// <summary>
/// LiteDB is configured with UtcDate=false, so a stored <see cref="DateTime"/> comes back as the
/// same instant but in Local kind. These two conversions keep every hosting date-only value
/// (PaidOn, UpcomingReminderSentForDueOn, DueReminderSentForDueOn) round-tripping correctly, and
/// keep reminder bookkeeping comparing DateOnly values rather than raw DateTimes.
/// </summary>
public static class HostingDates {
	public static DateTime ToStorage(DateOnly date) => DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

	public static DateOnly ToDateOnly(DateTime stored) => DateOnly.FromDateTime(stored.ToUniversalTime());
}
