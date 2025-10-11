using System.Text.Json;
using System.Text.Json.Serialization;

namespace PriceScraper.Serialisation;

public class StringToStringListConverter : JsonConverter<List<List<string>>>
{
    public override List<List<string>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        var result = new List<List<string>>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                result.Add([reader.GetString()!]);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                var inner = JsonSerializer.Deserialize<List<string>>(ref reader, options)!;
                result.Add(inner);
            }
            else
            {
                throw new JsonException();
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<List<string>> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var inner in value)
        {
            if (inner.Count == 1)
            {
                writer.WriteStringValue(inner[0]);
            }
            else
            {
                JsonSerializer.Serialize(writer, inner, options);
            }
        }

        writer.WriteEndArray();
    }
}

