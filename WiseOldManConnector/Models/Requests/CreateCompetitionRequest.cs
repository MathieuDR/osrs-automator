using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Requests; 

public class CreateCompetitionRequest {
    private CreateCompetitionRequest(string title, MetricType metric, DateTime start, DateTime end) {
        Title = title;
        Metric = metric.GetEnumValueNameOrDefault();
        Start = start;
        End = end;
    }

    public CreateCompetitionRequest(string title, MetricType metric, DateTime start, DateTime end,
        IEnumerable<string> participants) : this(title, metric, start, end) {
        Participants = participants;
    }

    public CreateCompetitionRequest(string title, MetricType metric, DateTime start, DateTime end,
        IEnumerable<Team> teams) : this(title, metric, start, end) {
        Teams = teams;
    }

    public CreateCompetitionRequest(string title, MetricType metric, DateTime start, DateTime end,
        Dictionary<string, IEnumerable<string>> teams) : this(title, metric, start, end) {
        Teams = teams.Select(x => new Team(x));
    }

    public CreateCompetitionRequest(string title, MetricType metric, DateTime start, DateTime end, int groupId,
        string verificationCode) : this(title, metric, start, end) {
        GroupId = groupId;
        VerificationCode = verificationCode;
    }

    public CreateCompetitionRequest(string title, MetricType metric, DateTime start, DateTime end, int groupId,
        string verificationCode, IEnumerable<string> participants) : this(title, metric, start, end, groupId,
        verificationCode) {
        Participants = participants;
    }

    public CreateCompetitionRequest(string title, MetricType metric, DateTime start, DateTime end, int groupId,
        string verificationCode, IEnumerable<Team> teams) : this(title, metric, start, end, groupId,
        verificationCode) {
        Teams = teams;
    }

    public CreateCompetitionRequest(string title, MetricType metric, DateTime start, DateTime end, int groupId,
        string verificationCode, Dictionary<string, IEnumerable<string>> teams) : this(title, metric, start, end,
        groupId, verificationCode) {
        Teams = teams.Select(x => new Team(x));
    }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("metric")]
    public string Metric { get; set; }

    [JsonProperty("startsAt")]
    public DateTime Start { get; set; }

    [JsonProperty("endsAt")]
    public DateTime End { get; set; }

    [JsonProperty("groupId")]
    public int GroupId { get; set; }

    [JsonProperty("groupVerificationCode")]
    public string VerificationCode { get; set; }

    [JsonProperty("participants")]
    public IEnumerable<string> Participants { get; set; }

    [JsonProperty("teams")]
    public IEnumerable<Team> Teams { get; set; }

    public class Team {
        public Team(string name, IEnumerable<string> participants) {
            Name = name;
            Participants = participants;
        }

        public Team(KeyValuePair<string, IEnumerable<string>> kvp) : this(kvp.Key, kvp.Value) { }


        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("participants")]
        public IEnumerable<string> Participants { get; set; }
    }
}