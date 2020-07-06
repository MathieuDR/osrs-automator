using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnectorTests.Fixtures;
using Xunit;
using Record = WiseOldManConnector.Models.Output.Record;

namespace WiseOldManConnectorTests.Connectors {
    public class RecordConnectorTests : ConnectorTests {
        public RecordConnectorTests(APIFixture fixture) : base(fixture) {
            _recordApi = fixture.ServiceProvider.GetService<IWiseOldManRecordApi>();
        }

        private readonly IWiseOldManRecordApi _recordApi;

        [Theory]
        [InlineData(new object[] {Period.Week, null})]
        [InlineData(new object[] {Period.Week, PlayerType.IronMan})]
        public async void RecordsByParametersResultsInCollectionOf20Records(Period period, PlayerType? playerType) {
            var metric = MetricType.TheCorruptedGauntlet;

            ConnectorCollectionResponse<Record> response = null;
            if (playerType.HasValue) {
                response = await _recordApi.View(metric, period, playerType.Value);
            } else  {
                response = await _recordApi.View(metric, period);
            }

            Assert.NotNull(response);
            Assert.Equal(20, response.Data.Count());
        }   

        [Fact]
        public async void RecordWithoutPeriodHasResultOf80Records() {
            var metric = MetricType.Fishing;

            var response = await _recordApi.View(metric);

            Assert.NotNull(response);
            Assert.Equal(80, response.Data.Count());
        }

        [Fact]
        public async void RecordByPeriodOnlyResultsOfSaidPeriod() {
            var metric = MetricType.Overall;
            var period = Period.Week;

            var response = await _recordApi.View(metric, period);
            
            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);

            Assert.Empty(response.Data.Where(x=>x.Period != period));
        }

        [Fact]
        public async void RecordByPlayerTypeOnlyResultsOfSaidPlayerType() {

            var metric = MetricType.Agility;
            var type = PlayerType.HardcoreIronMan;

            var response = await _recordApi.View(metric, type);
            
            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);

            Assert.Empty(response.Data.Where(x=> x.PlayerType != type));
        }
    }
}