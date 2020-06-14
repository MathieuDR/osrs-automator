using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.WiseOldMan;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces {
    public interface IWiseOldManCompetitionApi {
        #region Competition
        Task<ConnectorCollectionResponse<Competition>> SearchCompetition(Models.Requests.CompetitionRequest request);
        Task<ConnectorCollectionResponse<Competition>> Search(string title);
        Task<ConnectorCollectionResponse<Competition>> Search(string title, int playerId);
        Task<ConnectorCollectionResponse<Competition>> Search(string title, int playerId, MetricType metric);
        Task<ConnectorResponse<Competition>> View(int id);
        Task<ConnectorResponse<Competition>> Create(string title, MetricType metric, DateTimeOffset startsAt, DateTimeOffset endsAt, IEnumerable<string> participants);
        Task<ConnectorResponse<Competition>> Create(string title, MetricType metric, DateTimeOffset startsAt, DateTimeOffset endsAt, int groupId, string groupVerificationCode);
        Task<ConnectorResponse<Competition>> Edit(int id, string verificationCode, CompetitionRequest request);
        Task<ConnectorResponse<MessageResponse>> AddParticipants(int id, string verificationCode, IEnumerable<string> participants);
        Task<ConnectorResponse<MessageResponse>> RemoveParticipants(int id, string verificationCode, IEnumerable<string> participants);
        Task<ConnectorResponse<MessageResponse>> Delete(int id, string verificationCode);
        Task<ConnectorResponse<MessageResponse>> Update(int id);
        #endregion
    }
}