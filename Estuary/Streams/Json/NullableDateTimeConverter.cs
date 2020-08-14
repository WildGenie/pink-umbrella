using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Estuary.Streams.Json
{
    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            return string.IsNullOrWhiteSpace(str) ? null : new DateTime?(DateTime.Parse(str));
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options) => writer.WriteStringValue(value?.ToString());
    }
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => DateTime.Parse(reader.GetString());

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value);
    }
}