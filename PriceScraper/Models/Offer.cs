namespace PriceScraper.Models;

public class ScrapedOffer
{
    public required string IngredientId { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required decimal UnitPrice { get; init; }

    public required string BaseUnit { get; init; }

    public required string PublicationUrl { get; init; }

    public string? ImageUrl { get; init; }

    public decimal? Price { get; init; }

    public decimal? Savings { get; init; }
}
