using System.Net;
using System.Text.Json;
using PriceScraper;
using PriceScraper.Serialisation;
using PriceScraper.Services;
using Quartz;
using Quartz.Impl;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSystemd();
builder.Services
    .AddHttpClient("EreklambladClient", client =>
    {
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:144.0) Gecko/20100101 Firefox/144.0");
        client.DefaultRequestHeaders.Add("Accept", "*/*");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
        client.DefaultRequestHeaders.Add("Referer", "https://ereklamblad.se/");
        client.DefaultRequestHeaders.Add("Origin", "https://ereklamblad.se");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "cross-site");
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };
    });

var serializerOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};
serializerOptions.Converters.Add(new DateTimeOffsetConverter());
serializerOptions.Converters.Add(new NullableDecimalConverter());
builder.Services.AddSingleton(serializerOptions);

builder.Services.AddTransient<ScraperService>();
builder.Services.AddTransient<EreklambladService>();
builder.Services.AddQuartz(configurator =>
{
    var jobKey = new JobKey("ScraperJob");
    configurator.AddJob<ScraperJob>(opts => opts.WithIdentity(jobKey));

    if (builder.Environment.IsDevelopment())
    {
        configurator.AddTrigger(opts =>
            opts
                .ForJob(jobKey)
                .WithIdentity("ScraperDebugTrigger")
                .StartNow()
        );
    }

    configurator.AddTrigger(opts =>
        opts
            .ForJob(jobKey)
            .WithIdentity("ScraperTrigger")
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(7, 0))
    );
});
builder.Services.AddQuartzHostedService();

var host = builder.Build();
host.Run();
