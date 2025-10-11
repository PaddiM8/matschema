using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PriceScraper.Models;

namespace PriceScraper.Services;

public class EreklamBladService(
    IHttpClientFactory httpClientFactory,
    JsonSerializerOptions serializerOptions,
    ILogger<EreklamBladService> logger,
    IHostEnvironment environment
)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly JsonSerializerOptions _serializerOptions = serializerOptions;
    private readonly ILogger<EreklamBladService> _logger = logger;
    private readonly IHostEnvironment _environment = environment;

    public async Task<T?> QueryAsync<T>(object query)
    {
        var client = _httpClientFactory.CreateClient("EreklambladClient");
        var serialisedQuery = JsonSerializer.Serialize(query, _serializerOptions);
        var encodedQuery = Convert.ToBase64String(Encoding.UTF8.GetBytes(serialisedQuery));
        var body = new Dictionary<string, List<string>>
        {
            { "data", [encodedQuery] },
        };

        if (_environment.IsDevelopment())
            _logger.LogTrace("Request: {Request} (unencoded: {UnencodedQuery})", JsonSerializer.Serialize(body), serialisedQuery);

        var publicationsResponse = await client.PostAsync($"https://ereklamblad.se/", JsonContent.Create(body));

        if (_environment.IsDevelopment())
        {
            var stringValue = await publicationsResponse.Content.ReadAsStringAsync();
            _logger.LogTrace("Response: {Response}", stringValue);
        }

        var parsedResponse = await publicationsResponse.Content.ReadFromJsonAsync<EreklambladResponse<T>>(_serializerOptions);
        if (parsedResponse == null)
            return default;

        return parsedResponse.Value;
    }
}

