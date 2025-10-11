using System.Text.Json.Serialization;
using PriceScraper.Serialisation;

namespace PriceScraper.Models;

public class MealPlanItem
{
    public required string Name { get; init; }

    public string Notes { get; init; } = string.Empty;

    [JsonConverter(typeof(StringToStringListConverter))]
    public List<List<string>> Ingredients { get; init; } = [];
}

