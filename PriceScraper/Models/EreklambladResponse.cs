namespace PriceScraper.Models;

public class EreklambladResponse<T>
{
    public required string Key { get; init; }

    public required T Value { get; init; }
}

