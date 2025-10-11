namespace PriceScraper.Models;

public class EreklambladBusiness
{
    public required string Name { get; init; }

    public required string PublicId { get; init; }

    public required List<string> Slugs { get; init; }
}

