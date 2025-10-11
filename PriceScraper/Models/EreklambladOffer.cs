namespace PriceScraper.Models;

public record EreklambladOffer(
    string PublicId,
    string Name,
    string Description,
    decimal UnitPrice,
    string BaseUnit,
    decimal? Price,
    decimal? Savings
);
