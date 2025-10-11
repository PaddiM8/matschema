namespace PriceScraper.Models;

public record EreklambladOffer(
    string PublicId,
    string Name,
    string Description,
    decimal UnitPrice,
    string BaseUnit,
    string? Image,
    decimal? Price,
    decimal? Savings
);
