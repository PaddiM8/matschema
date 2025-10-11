namespace PriceScraper.Models;

public class EreklambladPublication
{
    public required string Id { get; init; }

    public DateTimeOffset ValidFrom { get; init; }

    public DateTimeOffset ValidUntil { get; init; }

    public bool IsValid
        => ValidFrom <= DateTimeOffset.Now && ValidUntil > DateTimeOffset.Now;
}

