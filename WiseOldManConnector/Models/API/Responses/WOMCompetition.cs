﻿using System;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses {
    internal class WOMCompetition : IResponse {
       
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("metric")]
            public string Metric { get; set; }

            [JsonProperty("score")]
            public int Score { get; set; }

            [JsonProperty("startsAt")]
            public DateTime StartsAt { get; set; }

            [JsonProperty("endsAt")]
            public DateTime EndsAt { get; set; }

            [JsonProperty("groupId")]
            public int? GroupId { get; set; }

            [JsonProperty("createdAt")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("updatedAt")]
            public DateTime UpdatedAt { get; set; }

            //[JsonProperty("group")]
            //public WOMGroup WOMGroup { get; set; }

            [JsonProperty("duration")]
            public string Duration { get; set; }

            [JsonProperty("participantCount")]
            public int ParticipantCount { get; set; }


            //[JsonProperty("totalGained")]
            //public int TotalGained { get; set; }

            //[JsonProperty("participants")]
            //public List<Participant> Participants { get; set; }
        
    }
}