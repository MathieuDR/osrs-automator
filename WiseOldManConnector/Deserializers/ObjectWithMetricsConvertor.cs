using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Deserializers {
    internal class ObjectWithMetricsConvertor<T, TU> : JsonConverter<T> where T : class, IMetricBearer<TU>, new() where TU : class, new() {
        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue,
            JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jsonObject = JObject.Load(reader);

            var obj = (existingValue ?? new T());

            // Populate the remaining standard properties
            using (var subReader = jsonObject.CreateReader())
            {
                serializer.Populate(subReader, obj);
            }
            
            var metrics = Enum.GetValues(typeof(MetricType)) as MetricType[];

            foreach (var type in metrics) {
                var member = type.GetEnumValueNameOrDefault();

                if (jsonObject.ContainsKey(member)) {
                    var metricToken = jsonObject[member];
                    var metric = metricToken?.ToObject<TU>();
                    obj.Metrics.Add(type, metric);
                }
            }

            return obj;
        }

        public override bool CanWrite => false;
    }
}