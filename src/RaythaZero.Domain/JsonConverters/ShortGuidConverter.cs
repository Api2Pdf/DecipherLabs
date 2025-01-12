using CSharpVitamins;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RaythaZero.Domain.JsonConverters;

public class ShortGuidConverter : JsonConverter<ShortGuid>
{
    public override ShortGuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            var value = reader.GetString();
            return new ShortGuid(value ?? string.Empty);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, ShortGuid value, JsonSerializerOptions options)
    {
        try
        {
            writer.WriteStringValue(value != null ? value.ToString() : string.Empty);
        }
        catch (Exception e)
        {
            writer.WriteStringValue(string.Empty);
        }
    }

    public override ShortGuid ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new ShortGuid(reader.GetString());

    public override void WriteAsPropertyName(Utf8JsonWriter writer, ShortGuid value, JsonSerializerOptions options)
        => writer.WritePropertyName(value.ToString());

}
