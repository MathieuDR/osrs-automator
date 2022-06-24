using FluentAssertions;
using MathieuDR.Common.Parsers;
using Xunit;

namespace DiscordBot.ServicesTests.Parser;

public class HumanizerTests {
    [Theory]
    [InlineData("8:30", "2021-12-01 08:30:00")]
    [InlineData("8h30m", "2021-12-01 08:30:00")]
    [InlineData("8h30", "2021-12-01 08:30:00")]
    [InlineData("8pm today", "2021-12-01 20:00:00")]
    [InlineData("today 8pm", "2021-12-01 20:00:00")]
    [InlineData("1 october", "2022-10-01 00:00:00")]
    [InlineData("first of october", "2022-10-01 00:00:00")]
    [InlineData("first of october 2023", "2023-10-01 00:00:00")]
    [InlineData("1st of october 2022", "2022-10-01 00:00:00")]
    [InlineData("at 6pm today", "2021-12-01 18:00:00")]
    [InlineData("2nd january", "2022-01-02 00:00:00")]
    [InlineData("2nd january 9h", "2022-01-02 09:00:00")]
    [InlineData("10th dec midnight", "2021-12-10 00:00:00")]
    [InlineData("10th december 10am", "2021-12-10 10:00:00")]
    [InlineData("2nd january 15h", "2022-01-02 15:00:00")]
    [InlineData("2nd january 15h30", "2022-01-02 15:30:00")]
    [InlineData("8pm", "2021-12-01 20:00:00")]
    [InlineData("15h", "2021-12-01 15:00:00")]
    [InlineData("at 15h", "2021-12-01 15:00:00")]
    [InlineData("I'll go back 8pm today", "2021-12-01 20:00:00")]
    [InlineData("14/12/2021 18:00", "2021-12-14 18:00:00")]
    public void CanParseDates(string dateString, string parseAbleDate) {
        var result = dateString.ToFutureDate(new DateTime(2021, 12, 1, 4, 10, 0));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(DateTime.Parse(parseAbleDate));
    }
}
