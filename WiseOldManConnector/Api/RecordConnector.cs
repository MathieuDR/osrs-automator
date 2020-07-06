using System;
using System.Threading.Tasks;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.API.Responses.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Api {
    internal class RecordConnector : BaseConnecter, IWiseOldManRecordApi {
        public RecordConnector(IServiceProvider provider) : base(provider) {
            Area = "records";
        }
        protected override string Area { get; }

        public Task<ConnectorCollectionResponse<Record>> View(MetricType metric) {
            return QueryRecords(metric);
        }

        public Task<ConnectorCollectionResponse<Record>> View(MetricType metric, Period period) {
            return QueryRecords(metric, period);
        }

        public Task<ConnectorCollectionResponse<Record>> View(MetricType metric, Period period, PlayerType playerType) {
            return QueryRecords(metric, period, playerType);
        }

        public Task<ConnectorCollectionResponse<Record>> View(MetricType metric, PlayerType playerType) {
            return QueryRecords(metric, null, playerType);
        }

        private async Task<ConnectorCollectionResponse<Record>> QueryRecords(MetricType metric, Period? period = null , PlayerType? playerType = null) {
            var request = GetNewRestRequest("/leaderboard");

            request.AddParameter("metric", metric.GetEnumValueNameOrDefault());

            if (playerType.HasValue) {
                request.AddParameter("playerType", playerType.Value.GetEnumValueNameOrDefault());
            }

            if (period.HasValue) {
                request.AddParameter("period", period.Value.GetEnumValueNameOrDefault());
                var queryResponse = await ExecuteCollectionRequest<WOMRecord>(request);
                var response =  GetResponse<WOMRecord, Record>(queryResponse);

                foreach (Record record in response.Data) {
                    record.Period = period.Value;
                }

                return response;
            } else {
                var response = await ExecuteRequest<RecordResponse>(request);
                return GetCollectionResponse<Record>(response);
            }

           
        }
    }
}