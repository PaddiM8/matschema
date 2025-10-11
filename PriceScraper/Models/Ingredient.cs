namespace PriceScraper.Models;

public class Ingredient
{
    public required List<string> Queries { get; init; }

    public List<string> ExclusionQueries { get; init; } = [];

    public List<string> DescriptionQueries { get; init; } = [];

    public bool AlwaysRelevant { get; init; }

    public int? MaxUnitPrice { get; init; }

    public int? MaxWeeksBuyInAdvance { get; init; }
}

