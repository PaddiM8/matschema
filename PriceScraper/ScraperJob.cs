using System.Globalization;
using System.Text.Json;
using PriceScraper.Models;
using PriceScraper.Services;
using Quartz;

namespace PriceScraper;

public class ScraperJob(
    IConfiguration configuration,
    ScraperService scraperService,
    IHostEnvironment environment,
    JsonSerializerOptions serializerOptions
) : IJob
{
    private static readonly Random _random = new();

    private readonly IConfiguration _configuration = configuration;
    private readonly ScraperService _scraperService = scraperService;
    private readonly IHostEnvironment _environment = environment;
    private readonly JsonSerializerOptions _serializerOptions = new(serializerOptions)
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public async Task Execute(IJobExecutionContext context)
    {
        if (_environment.IsProduction())
        {
            await Task.Delay(
                TimeSpan.FromMinutes(_random.Next(0, 60))
            );
        }

        var storagePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            _configuration["StoragePath"]!.ToString()
        );
        var date = DateTime.Now;
        var weekNumber = CultureInfo
            .InvariantCulture
            .Calendar
            .GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        var fileName = $"{date.Year}-{weekNumber}.json";
        var filePath = Path.Combine(storagePath, fileName);
        if (File.Exists(filePath))
        {
            return;
        }

        Directory.CreateDirectory(storagePath);

        var storeOptions = _configuration
            .GetRequiredSection("Stores")
            .Get<List<StoreOption>>()!;
        var result = await _scraperService.ScrapeAsync(storeOptions);
        File.WriteAllText(filePath, JsonSerializer.Serialize(result, _serializerOptions));

        var latestPath = Path.Combine(storagePath, "latest.json");
        if (File.Exists(latestPath))
            File.Delete(latestPath);

        File.CreateSymbolicLink(latestPath, filePath);
    }
}

