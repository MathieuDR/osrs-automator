using System;
using System.Collections.Generic;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBotFanatic.Helpers.JsonConverters {
    public class LeaderboardConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return (objectType == typeof(List<Record>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer) {
            JToken token = JToken.Load(reader);
            switch (token.Type) {
                case JTokenType.Array:
                    return new LeaderboardResponse() {Members = token.ToObject<List<LeaderboardMember>>()};
                case JTokenType.Object:
                    var defaultCreator = serializer.ContractResolver.ResolveContract(objectType).DefaultCreator;
                    if (defaultCreator != null) {
                        existingValue ??= defaultCreator();
                    } else {
                        throw new NullReferenceException(nameof(defaultCreator));
                    }

                    serializer.Populate(token.CreateReader(), existingValue);
                    return existingValue;
                case JTokenType.Null:
                    return null;
                default: {
                    throw new JsonSerializationException();
                }
            }
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}