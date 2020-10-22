﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces {
    public interface IWiseOldManCompetitionApi {

        Task<ConnectorCollectionResponse<Competition>> SearchCompetition(Models.Requests.CompetitionRequest request);
        Task<ConnectorCollectionResponse<Competition>> Search(string title);
        Task<ConnectorCollectionResponse<Competition>> Search(string title, int limit, int offset);
        Task<ConnectorCollectionResponse<Competition>> Search(string title, MetricType metric);
        Task<ConnectorCollectionResponse<Competition>> Search(string title, MetricType metric, int limit, int offset);
        Task<ConnectorCollectionResponse<Competition>> Search(string title, MetricType metric, CompetitionStatus status);
        Task<ConnectorCollectionResponse<Competition>> Search(string title, MetricType metric, CompetitionStatus status, int limit, int offset);
        Task<ConnectorCollectionResponse<Competition>> Search(string title, CompetitionStatus status);
        Task<ConnectorCollectionResponse<Competition>> Search(string title, CompetitionStatus status, int limit, int offset);
        Task<ConnectorResponse<Competition>> View(int id);
        Task<ConnectorResponse<Competition>> Create(string title, MetricType metric, DateTimeOffset startsAt, DateTimeOffset endsAt, IEnumerable<string> participants);
        Task<ConnectorResponse<Competition>> Create(string title, MetricType metric, DateTimeOffset startsAt, DateTimeOffset endsAt, int groupId, string groupVerificationCode);
        Task<ConnectorResponse<Competition>> Edit(int id, string verificationCode, CompetitionRequest request);
        Task<ConnectorResponse<MessageResponse>> AddParticipants(int id, string verificationCode, IEnumerable<string> participants);
        Task<ConnectorResponse<MessageResponse>> RemoveParticipants(int id, string verificationCode, IEnumerable<string> participants);
        Task<ConnectorResponse<MessageResponse>> Delete(int id, string verificationCode);
        Task<ConnectorResponse<MessageResponse>> Update(int id);

    }
}