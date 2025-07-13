using System.Text.Json;
using System.Text.Json.Serialization;

namespace Recycler.API.Converters;

public class CaseForRawMaterialsConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
            return value;

        return char.ToUpper(value[0]) + value.Substring(1);
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        // Optional: replace back or keep as-is
        writer.WriteStringValue(value);
    }
}