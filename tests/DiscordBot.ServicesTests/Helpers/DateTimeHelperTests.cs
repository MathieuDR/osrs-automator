using DiscordBot.Services.Helpers;
using FluentAssertions;
using TimeZoneConverter;
using Xunit;
using Xunit.Abstractions;

namespace DiscordBot.ServicesTests.Helpers;

public class DateTimeHelperTests {
    private readonly ITestOutputHelper _testOutputHelper;

    private const string UTCTimeKey = "UTC";
    private const string WesternEuTimeKey = "W. Europe Standard Time";
    private const string HawaiianTimeKey = "Hawaiian Standard Time";

    public DateTimeHelperTests(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void CanGetCorrectDateWithNoOffset() {
        var date = new DateTime(2021, 11, 21, 14, 0, 0);
        var dateTimeOffset = date.GetCorrectDateTimeOffset(UTCTimeKey);

        dateTimeOffset.Should().NotBe(default);
        dateTimeOffset.Offset.Should().Be(TimeSpan.Zero);
    }
    
    [Fact]
    public void CanGetCorrectDateWithNoOffsetForNullKey() {
        var date = new DateTime(2021, 11, 21, 14, 0, 0);
        var dateTimeOffset = date.GetCorrectDateTimeOffset(null);

        dateTimeOffset.Should().NotBe(default);
        dateTimeOffset.Offset.Should().Be(TimeSpan.Zero);
    }
    
    [Fact]
    public void CanGetCorrectDateWithNoOffsetForEmptyKey() {
        var date = new DateTime(2021, 11, 21, 14, 0, 0);
        var dateTimeOffset = date.GetCorrectDateTimeOffset("");

        dateTimeOffset.Should().NotBe(default);
        dateTimeOffset.Offset.Should().Be(TimeSpan.Zero);
    }
    
    [Fact(Skip = "It does have DST now")]
    public void HawaiianStandardTimeDoesNotHaveDST() {
        var tz = TZConvert.GetTimeZoneInfo(HawaiianTimeKey);
        tz.SupportsDaylightSavingTime.Should().BeFalse();
    }
    
    [Fact]
    public void WesternEuropeStandardTimeDoesHaveDST() {
        var tz = TZConvert.GetTimeZoneInfo(WesternEuTimeKey);
        tz.SupportsDaylightSavingTime.Should().BeTrue();
    }

    [Fact(Skip = "Incorrect test")]
    public void CanGetCorrectDateWithOffsetWithInactiveDST() {
        var date = new DateTime(2021, 11, 21, 14, 0, 0);
        var dateTimeOffset = date.GetCorrectDateTimeOffset(WesternEuTimeKey);

        dateTimeOffset.Should().NotBe(default);
        dateTimeOffset.Offset.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact(Skip = "Incorrect test")]
    public void CanGetCorrectDateWithOffsetWithActiveDST() {
        var date = new DateTime(2021, 10, 01, 14, 0, 0);
        var dateTimeOffset = date.GetCorrectDateTimeOffset(WesternEuTimeKey);

        dateTimeOffset.Should().NotBe(default);
        dateTimeOffset.Offset.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void CanGetCorrectDateWithOffsetWithoutInactiveDST() {
        var date = new DateTime(2021, 11, 21, 14, 0, 0);
        var dateTimeOffset = date.GetCorrectDateTimeOffset(HawaiianTimeKey);

        dateTimeOffset.Should().NotBe(default);
        dateTimeOffset.Offset.Should().Be(TimeSpan.FromHours(-10));
    }

    [Fact]
    public void CanGetCorrectDateWithOffsetWithoutActiveDST() {
        var date = new DateTime(2021, 10, 01, 14, 0, 0);
        var dateTimeOffset = date.GetCorrectDateTimeOffset(HawaiianTimeKey);

      
        dateTimeOffset.Should().NotBe(default);
        dateTimeOffset.Offset.Should().Be(TimeSpan.FromHours(-10));
    }
}
