using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using Delta = WiseOldManConnector.Models.Output.Delta;
using Record = WiseOldManConnector.Models.Output.Record;
using Snapshot = WiseOldManConnector.Models.Output.Snapshot;

namespace WiseOldManConnector.Api {
    internal class PlayerConncetor : BaseConnecter, IWiseOldManPlayerApi {
        public PlayerConncetor(IServiceProvider provider) : base(provider) {
            Area = "players";
        }

        protected override string Area { get; }

        #region search

        public async Task<ConnectorCollectionResponse<Player>> Search(string username) {
            var request = GetNewRestRequest("search");
            request.AddParameter("username", username);

            var result = await ExecuteCollectionRequest<SearchResponse>(request);
            return GetResponse<Player>(result);
        }

        #endregion

        #region track

        public async Task<ConnectorResponse<Player>> Track(string username) {
            var request = GetNewRestRequest("track");
            request.AddJsonBody(new {username});

            var result = await ExecuteRequest<PlayerResponse>(request);
            return GetResponse<Player>(result);
        }

        #endregion

        #region import

        public async Task<ConnectorResponse<MessageResponse>> Import(string username) {
            var request = GetNewRestRequest("track");
            request.AddJsonBody(new {username});

            var result = await ExecuteRequest<WOMMessageResponse>(request);
            return GetResponse<MessageResponse>(result);
        }

        

        #endregion

        #region competitions

        public async Task<ConnectorCollectionResponse<Competition>> Competitions(int id) {
            var request = GetNewRestRequest("/{{id}}/competitions");
            request.AddParameter("id", id, ParameterType.UrlSegment);

            var result = await ExecuteCollectionRequest<CompetitionResponse>(request);
            return GetResponse<Competition>(result);
        }

        public async Task<ConnectorCollectionResponse<Competition>> Competitions(string username) {
            throw new NotImplementedException();
        }

        #endregion

        #region asserting

        public async Task<ConnectorResponse<MessageResponse>> AssertPlayerType(string username) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<MessageResponse>> AssertDisplayName(string username) {
            throw new NotImplementedException();
        }

        #endregion

        #region achievements

        public async Task<ConnectorCollectionResponse<Achievement>> Achievements(int id) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Achievement>> Achievements(int id, bool includeMissing) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Achievement>> Achievements(string username) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Achievement>> Achievements(string username, bool includeMissing) {
            throw new NotImplementedException();
        }

        #endregion

        #region snapshots

        public async Task<ConnectorCollectionResponse<Snapshot>> Snapshots(int id) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Snapshot>> Snapshots(int id, Period period) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Snapshot>> Snapshots(string username) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Snapshot>> Snapshots(string username, Period period) {
            throw new NotImplementedException();
        }

        #endregion

        #region gained

        public async Task<ConnectorCollectionResponse<Delta>> Gained(int id) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<Delta>> Gained(int id, Period period) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Delta>> Gained(string username) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<Delta>> Gained(string username, Period period) {
            throw new NotImplementedException();
        }

        #endregion

        #region records

        public async Task<ConnectorCollectionResponse<Record>> Records(int id) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Record>> Records(int id, MetricType metric) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Record>> Records(int id, Period period) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Record>> Records(int id, MetricType metric, Period period) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Record>> Records(string username) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Record>> Records(string username, MetricType metric) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Record>> Records(string username, Period period) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorCollectionResponse<Record>> Records(string username, MetricType metric, Period period) {
            throw new NotImplementedException();
        }

        #endregion

        #region view

        public async Task<ConnectorResponse<Player>> View(string username) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<Player>> View(int id) {
            throw new NotImplementedException();
        }

        #endregion
    }
}