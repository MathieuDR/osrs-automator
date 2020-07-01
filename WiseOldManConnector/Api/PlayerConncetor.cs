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
            request.Method = Method.POST;
            username = username.ToLowerInvariant();
            request.AddJsonBody(new {username});

            PlayerResponse result = await ExecuteRequest<PlayerResponse>(request);
            return GetResponse<Player>(result);
        }

        #endregion

        #region import

        public async Task<ConnectorResponse<MessageResponse>> Import(string username) {
            var request = GetNewRestRequest("import");
            request.AddJsonBody(new {username});

            var result = await ExecuteRequest<WOMMessageResponse>(request);
            return GetResponse<MessageResponse>(result);
        }

        

        #endregion

        #region competitions

        public async Task<ConnectorCollectionResponse<Competition>> Competitions(int id) {
            var request = GetNewRestRequest("/{id}/competitions");
            request.AddParameter("id", id, ParameterType.UrlSegment);

            var result = await ExecuteCollectionRequest<CompetitionResponse>(request);
            return GetResponse<Competition>(result);
        }

        public async Task<ConnectorCollectionResponse<Competition>> Competitions(string username) {
            var request = GetNewRestRequest("/username/{username}/competitions");
            request.AddParameter("username", username, ParameterType.UrlSegment);

            var result = await ExecuteCollectionRequest<CompetitionResponse>(request);
            return GetResponse<Competition>(result);
        }

        #endregion

        #region asserting

        public async Task<ConnectorResponse<MessageResponse>> AssertPlayerType(string username) {
            var request = GetNewRestRequest("/assert-type");
            request.AddJsonBody(new {username});

            var result = await ExecuteRequest<WOMMessageResponse>(request);
            return GetResponse<MessageResponse>(result);
        }

        public async Task<ConnectorResponse<MessageResponse>> AssertDisplayName(string username) {
            var request = GetNewRestRequest("/assert-name");
            request.AddJsonBody(new {username});

            var result = await ExecuteRequest<WOMMessageResponse>(request);
            return GetResponse<MessageResponse>(result);
        }

        #endregion

        #region achievements

        public Task<ConnectorCollectionResponse<Achievement>> Achievements(int id) {
            return Achievements(id, false);
        }

        public async Task<ConnectorCollectionResponse<Achievement>> Achievements(int id, bool includeMissing) {
            var request = GetNewRestRequest("/{id}/achievements");
            request.AddParameter("id", id, ParameterType.UrlSegment);
            request.AddParameter("includeMissing", includeMissing, ParameterType.UrlSegment);

            var result = await ExecuteRequest<AchievementResponse>(request);
            return GetCollectionResponse<Achievement>(result);
        }

        public Task<ConnectorCollectionResponse<Achievement>> Achievements(string username) {
            return Achievements(username, false);
        }

        public async Task<ConnectorCollectionResponse<Achievement>> Achievements(string username, bool includeMissing) {
            var request = GetNewRestRequest("/username/{username}/achievements");
            request.AddParameter("username", username, ParameterType.UrlSegment);
            request.AddParameter("includeMissing", includeMissing, ParameterType.UrlSegment);

            var result = await ExecuteRequest<AchievementResponse>(request);
            return GetCollectionResponse<Achievement>(result);
        }

        #endregion

        #region snapshots

        public async Task<ConnectorResponse<Snapshots>> Snapshots(int id) {
            var request = GetNewRestRequest("/{id}/snapshots");
            request.AddParameter("id", id, ParameterType.UrlSegment);
            
            var result = await ExecuteRequest<SnapshotsResponse>(request);


            return GetResponse<Snapshots>(result);
        }

        public async Task<ConnectorResponse<Snapshots>> Snapshots(int id, Period period) {
            var request = GetNewRestRequest("/{id}/snapshots");
            request.AddParameter("id", id, ParameterType.UrlSegment);
            request.AddParameter("period", period, ParameterType.UrlSegment);

            var result = await ExecuteRequest<SnapshotsResponse>(request);
            return GetResponse<Snapshots>(result);
        }

        public async Task<ConnectorResponse<Snapshots>> Snapshots(string username) {
            var request = GetNewRestRequest("/username/{username}/snapshots");
            request.AddParameter("username", username, ParameterType.UrlSegment);

            var result = await ExecuteRequest<PeriodSpecificSnapshotsResponse>(request);
            return GetResponse<Snapshots>(result);
        }

        public async Task<ConnectorResponse<Snapshots>> Snapshots(string username, Period period) {
            var request = GetNewRestRequest("/username/{username}/snapshots");
            request.AddParameter("username", username, ParameterType.UrlSegment);
            request.AddParameter("period", period, ParameterType.UrlSegment);

            var result = await ExecuteRequest<PeriodSpecificSnapshotsResponse>(request);
            return GetResponse<Snapshots>(result);
        }

        #endregion

        #region gained

        public async Task<ConnectorCollectionResponse<Delta>> Gained(int id) {
            var request = GetNewRestRequest("/{id}/gained");
            request.AddParameter("id", id, ParameterType.UrlSegment);
            
            var result = await ExecuteRequest<DeltaFullResponse>(request);


            return GetCollectionResponse<Delta>(result);
        }

        public async Task<ConnectorResponse<Delta>> Gained(int id, Period period) {
            var request = GetNewRestRequest("/{id}/gained");
            request.AddParameter("id", id, ParameterType.UrlSegment);
            request.AddParameter("period", period, ParameterType.UrlSegment);

            var result = await ExecuteRequest<DeltaResponse>(request);
            return GetResponse<Delta>(result);
        }

        public async Task<ConnectorCollectionResponse<Delta>> Gained(string username) {
            var request = GetNewRestRequest("/username/{username}/gained");
            request.AddParameter("username", username, ParameterType.UrlSegment);

            var result = await ExecuteRequest<DeltaResponse>(request);
            return GetCollectionResponse<Delta>(result);
        }

        public async Task<ConnectorResponse<Delta>> Gained(string username, Period period) {
            var request = GetNewRestRequest("/username/{username}/gained");
            request.AddParameter("username", username, ParameterType.UrlSegment);
            request.AddParameter("period", period, ParameterType.UrlSegment);

            var result = await ExecuteRequest<DeltaFullResponse>(request);
            return GetResponse<Delta>(result);
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
            var request = GetNewRestRequest("username/{username}");
            request.AddParameter("username", username, ParameterType.UrlSegment);
            request.Method = Method.GET;

            var result = await ExecuteRequest<PlayerResponse>(request);
            return GetResponse<Player>(result);
        }

        public async Task<ConnectorResponse<Player>> View(int id) {
            var request = GetNewRestRequest("{id}");
            request.AddParameter("id", id, ParameterType.UrlSegment);

            var result = await ExecuteRequest<PlayerResponse>(request);
            return GetResponse<Player>(result);
        }

        #endregion
    }
}