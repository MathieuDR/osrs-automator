using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnectorTests.Fixtures;
using Xunit;
using Record = WiseOldManConnector.Models.Output.Record;

namespace WiseOldManConnectorTests.Connectors {
    public class RecordConnectorTests : ConnectorTests {
        private readonly IWiseOldManRecordApi _recordApi;

        public RecordConnectorTests(ApiFixture fixture) : base(fixture) {
            _recordApi = fixture.ServiceProvider.GetService<IWiseOldManRecordApi>();
        }

        [Theory]
        [InlineData(Period.Week, null)]
        [InlineData(Period.Week, PlayerType.IronMan)]
        public async Task RecordsByParametersResultsInCollectionOf20Records(Period period, PlayerType? playerType) {
            var metric = MetricType.TheCorruptedGauntlet;

            ConnectorCollectionResponse<Record> response = null;
            if (playerType.HasValue) {
                response = await _recordApi.View(metric, period, playerType.Value);
            } else {
                response = await _recordApi.View(metric, period);
            }

            Assert.NotNull(response);
            Assert.Equal(20, response.Data.Count());
        }

        [Fact]
        public async Task RecordByPeriodOnlyResultsOfSaidPeriod() {
            var metric = MetricType.Overall;
            var period = Period.Week;

            var response = await _recordApi.View(metric, period);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);

            Assert.Empty(response.Data.Where(x => x.Period != period));
        }

        [Fact]
        public async Task RecordByPlayerTypeOnlyResultsOfSaidPlayerType() {
            var metric = MetricType.Agility;
            var type = PlayerType.HardcoreIronMan;
            var period = Period.Month;

            var response = await _recordApi.View(metric, period, type);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);

            Assert.Empty(response.Data.Where(x => x.Player.Type != type));
        }

        [Fact]
        public async Task RecordByParametersResultInRecordsWithDisplayName() {
            var metric = MetricType.Agility;
            var type = PlayerType.HardcoreIronMan;
            var period = Period.Month;

            var response = await _recordApi.View(metric, period, type);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);

            Assert.Empty(response.Data.Where(x => string.IsNullOrWhiteSpace(x.Player.DisplayName)));
        }
    }
}
