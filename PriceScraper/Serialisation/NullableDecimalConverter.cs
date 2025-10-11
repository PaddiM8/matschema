using System.Text.Json;
using System.Text.Json.Serialization;

namespace PriceScraper.Serialisation;

public class NullableDecimalConverter : JsonConverter<decimal?>
{
    public override bool HandleNull
        => true;

    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value ?? 0);
    }
}

