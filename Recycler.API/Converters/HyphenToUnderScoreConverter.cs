using System.Text.Json;
using System.Text.Json.Serialization;

namespace Recycler.API.Converters;

public class HyphenToUnderscoreConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value?.Replace('-', '_');
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        // Optional: replace back or keep as-is
        writer.WriteStringValue(value);
    }
}