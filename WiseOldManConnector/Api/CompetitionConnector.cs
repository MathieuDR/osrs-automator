using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Api {
    internal class CompetitionConnector : BaseConnecter,  IWiseOldManCompetitionApi{
        public CompetitionConnector(IServiceProvider provider) : base(provider) {
            Area = "competitions";
        }
        protected override string Area { get; }
        public Task<ConnectorCollectionResponse<Competition>> SearchCompetition(CompetitionRequest request) {
            throw new NotImplementedException();
        }

        public Task<ConnectorCollectionResponse<Competition>> Search(string title) {
            throw new NotImplementedException();
        }

        public Task<ConnectorCollectionResponse<Competition>> Search(string title, int limit, int offset) {
            throw new NotImplementedException();
        }

        public Task<ConnectorCollectionResponse<Competition>> Search(string title, MetricType metric) {
            throw new NotImplementedException();
        }

        public Task<ConnectorCollectionResponse<Competition>> Search(string title, MetricType metric, int limit, int offset) {
            throw new NotImplementedException();
        }

        public Task<ConnectorCollectionResponse<Competition>> Search(string title, MetricType metric, CompetitionStatus status) {
            throw new NotImplementedException();
        }

        public Task<ConnectorCollectionResponse<Competition>> Search(string title, MetricType metric, CompetitionStatus status, int limit, int offset) {
            throw new NotImplementedException();
        }

        public Task<ConnectorCollectionResponse<Competition>> Search(string title, CompetitionStatus status) {
            throw new NotImplementedException();
        }

        public Task<ConnectorCollectionResponse<Competition>> Search(string title, CompetitionStatus status, int limit, int offset) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<Competition>> View(int id) {
            var request = GetNewRestRequest("{id}");
            request.AddParameter("id", id, ParameterType.UrlSegment);

            var result = await ExecuteRequest<WOMCompetition>(request);
            return GetResponse<Competition>(result);
        }

        public Task<ConnectorResponse<Competition>> Create(string title, MetricType metric, DateTimeOffset startsAt, DateTimeOffset endsAt, IEnumerable<string> participants) {
            throw new NotImplementedException();
        }

        public Task<ConnectorResponse<Competition>> Create(string title, MetricType metric, DateTimeOffset startsAt, DateTimeOffset endsAt, int groupId,
            string groupVerificationCode) {
            throw new NotImplementedException();
        }

        public Task<ConnectorResponse<Competition>> Edit(int id, string verificationCode, CompetitionRequest request) {
            throw new NotImplementedException();
        }

        public Task<ConnectorResponse<MessageResponse>> AddParticipants(int id, string verificationCode, IEnumerable<string> participants) {
            throw new NotImplementedException();
        }

        public Task<ConnectorResponse<MessageResponse>> RemoveParticipants(int id, string verificationCode, IEnumerable<string> participants) {
            throw new NotImplementedException();
        }

        public Task<ConnectorResponse<MessageResponse>> Delete(int id, string verificationCode) {
            throw new NotImplementedException();
        }

        public Task<ConnectorResponse<MessageResponse>> Update(int id) {
            throw new NotImplementedException();
        }
    }
}