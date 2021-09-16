using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Xunit;

namespace DiscordBot.ServicesTests.Parser {
    public class HumanizerTests {
        private DateTime Reference = new(2021, 12, 1, 4, 10, 0);

        [Theory]
        //[InlineData("8:30h", "2021-12-01 08:30:00")]
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
            var recognized = DateTimeRecognizer.RecognizeDateTime(dateString, Culture.English, refTime: Reference);

            DateTime? baseDate = null;
            string durationString = null;
            foreach (var modelResult in recognized) {
                var values = modelResult.Resolution["values"] as List<Dictionary<string, string>> ??
                             new List<Dictionary<string, string>>();

                foreach (var dict in values) {
                    var type = dict["type"];

                    switch (type) {
                        case "time":
                            baseDate ??= Reference.Date.Add(TimeSpan.Parse(dict["value"]));
                            break;
                        case "datetime":
                        case "date":
                            baseDate = DateTime.Parse(dict["value"]);
                            break;
                        case "datetimerange":
                        case "daterange":
                        case "timerange":
                            baseDate = DateTime.Parse(dict["start"]);
                            break;
                        case "duration":
                            durationString = dict["value"];
                            break;
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(durationString)) {
                baseDate ??= Reference.Date;
                baseDate =  baseDate.Value.Add(TimeSpan.FromSeconds(long.Parse(durationString)));
            }
            
            if (!baseDate.HasValue) {
                throw new InvalidOperationException("No base string");
            }

            var result = baseDate.Value;
            if (result <= Reference) {
                result = result.AddYears(1);
            }

            recognized.Should().HaveCountGreaterOrEqualTo(1);
            result.Should().Be(DateTime.Parse(parseAbleDate));
        }
    }
}
