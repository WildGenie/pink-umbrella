using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Estuary.Streams.Json
{
    public class JsonStringEnumConverter<T> : JsonConverter<T> where T : struct
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => (T)Enum.Parse(typeof(T), reader.GetString(), true);

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }
}