using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordBot.Common.JsonConverters;

public sealed class EnumAttributeConverter<TEnum> : JsonConverter<TEnum> {
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String) {
            var enumString = reader.GetString();
            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)) {
                if (Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) is EnumMemberAttribute attribute) {
                    if (attribute.Value == enumString) {
                        return (TEnum)field.GetValue(null);
                    }
                }
            }
        }

        throw new JsonException($"Unable to convert \"{reader.GetString()}\" to enum {typeof(TEnum).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) {
        throw new NotImplementedException();
    }
}
