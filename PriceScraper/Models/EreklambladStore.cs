namespace PriceScraper.Models;

public class EreklambladStore
{
    public required EreklambladBusiness Business { get; init; }

    public List<EreklambladPublication> Publications { get; init; } = [];
}
