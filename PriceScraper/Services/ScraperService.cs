using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using PriceScraper.Models;

namespace PriceScraper.Services;

public class ScraperService(
    EreklambladService ereklambladService,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    JsonSerializerOptions serializerOptions,
    ILogger<ScraperService> logger
)
{
    private readonly EreklambladService _ereklamBladService = ereklambladService;
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly JsonSerializerOptions _serializerOptions = serializerOptions;
    private readonly ILogger<ScraperService> _logger = logger;

    public async Task<Dictionary<string, List<ScrapedOffer>>> ScrapeAsync(IEnumerable<StoreOption> storeOptions)
    {
        var offersByStore = new Dictionary<string, List<ScrapedOffer>>();
        foreach (var storeOption in storeOptions)
        {
            var (store, offers) = await ScrapeStoreAsync(storeOption);
            if (store == null)
                continue;

            var slug = store.Business.Slugs.FirstOrDefault();
            if (slug == null)
                continue;

            offersByStore[slug] = offers;
        }

        return offersByStore;
    }

    public async Task<(EreklambladStore? store, List<ScrapedOffer> offers)> ScrapeStoreAsync(StoreOption storeOption)
    {
        _logger.LogInformation("Scraping store: {StoreId}", storeOption.Id);

        var businessIdQuery = new List<object>
        {
            "business",
            new
            {
                countryCode = "SE",
                slug = storeOption.Name,
            },
        };

        var business = await _ereklamBladService.QueryAsync<EreklambladBusiness>(businessIdQuery);
        if (business == null)
            return (null, []);

        var query = new List<object>
        {
            "fronts",
            new
            {
                businessIds = new List<string> { business.PublicId },
                localBusinessIds = new List<string> { storeOption.Id },
            },
        };

        var stores = await _ereklamBladService.QueryAsync<List<EreklambladStore>>(query);
        var store = stores?.FirstOrDefault();
        if (store == null)
            return (null, []);

        var publication = store
            .Publications
            .FirstOrDefault(x => x.IsValid);
        if (publication == null)
        {
            _logger.LogInformation("No valid publication was found");

            return (store, []);
        }

        _logger.LogInformation("Found publication {PublicationId}", publication.Id);

        var ingredientsPath = Path.Combine(Directory.GetCurrentDirectory(), _configuration["IngredientsPath"]!);
        var ingredients = JsonSerializer.Deserialize<Dictionary<string, Ingredient>>(File.ReadAllText(ingredientsPath), _serializerOptions)!;

        var mealPlanPath = Path.Combine(Directory.GetCurrentDirectory(), _configuration["MealPlanPath"]!);
        var mealPlan = JsonSerializer.Deserialize<Dictionary<int, MealPlanWeek>>(File.ReadAllText(mealPlanPath), _serializerOptions)!;

        var date = DateTime.Now;
        var currentWeek = CultureInfo
            .InvariantCulture
            .Calendar
            .GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        var pageNumber = 1;
        var offers = new List<ScrapedOffer>();
        var client = _httpClientFactory.CreateClient("EreklambladClient");
        while (true)
        {
            var publicationResponse = await client.GetAsync($"https://publication-viewer.tjek.com/api/paged-publications/{publication.Id}/{pageNumber}");
            var responseNode = await publicationResponse.Content.ReadFromJsonAsync<JsonNode>(_serializerOptions);
            var pageHotspots = responseNode?["hotspots"]?.Deserialize<List<TjekHotspot>>(_serializerOptions);
            if (pageHotspots?.Count is 0 or null)
                break;

            foreach (var hotspot in pageHotspots)
            {
                _logger.LogTrace("Resolving hotspot {HotspotName}", hotspot.Offer.Name);
                var resolvedOffers = await ResolveOffer(
                    storeOption,
                    publication.Id,
                    hotspot.Offer,
                    ingredients,
                    mealPlan,
                    currentWeek
                );

                if (resolvedOffers.Count > 0)
                    _logger.LogTrace("Resolved offers: {OfferNames}", string.Join(", ", resolvedOffers.Select(x => x.Name)));

                offers.AddRange(resolvedOffers);
            }

            pageNumber++;
        }

        return (store, offers);
    }

    private async Task<List<ScrapedOffer>> ResolveOffer(
        StoreOption storeOption,
        string publicationId,
        TjekOffer tjekOffer,
        Dictionary<string, Ingredient> ingredients,
        Dictionary<int, MealPlanWeek> mealPlan,
        int currentWeek
    )
    {
        var matchedIngredients = new List<(string id, Ingredient value, int? relevantForWeek)>();
        foreach (var ingredient in ingredients)
        {
            var isMatch = ingredient
                .Value
                .Queries
                .Any(query => Regex.Match(tjekOffer.Name, query, RegexOptions.IgnoreCase).Success);
            var isExclusionMatch = ingredient
                .Value
                .ExclusionQueries
                .Any(query => Regex.Match(tjekOffer.Name, query, RegexOptions.IgnoreCase).Success);
            if (!isMatch || (ingredient.Value.ExclusionQueries.Count > 0 && isExclusionMatch))
                continue;

            var (isRelevant, relevantForWeek) = IngredientIsRelevant(ingredient.Key, ingredient.Value, mealPlan, currentWeek);
            if (isRelevant)
                matchedIngredients.Add((ingredient.Key, ingredient.Value, relevantForWeek));
        }

        if (matchedIngredients.Count == 0)
            return [];

        var query = new List<object>
        {
            "offer",
            new
            {
                publicId = tjekOffer.Id,
            },
        };

        var offer = await _ereklamBladService.QueryAsync<EreklambladOffer>(query);
        if (offer == null)
            return [];

        var result = new List<ScrapedOffer>();
        foreach (var (ingredientId, ingredient, relevantForWeek) in matchedIngredients)
        {
            if (ingredient.MaxUnitPrice.HasValue && offer.UnitPrice > ingredient.MaxUnitPrice.Value)
                continue;

            var isDescriptionMatch = ingredient
                .DescriptionQueries
                .Any(query => Regex.Match(offer.Description, query, RegexOptions.IgnoreCase).Success);
            if (ingredient.DescriptionQueries.Count > 0 && !isDescriptionMatch)
                continue;

            var scrapedOffer = new ScrapedOffer
            {
                IngredientId = ingredientId,
                Name = offer.Name,
                Description = offer.Description,
                UnitPrice = offer.UnitPrice,
                BaseUnit = offer.BaseUnit,
                ImageUrl = offer.Image,
                PublicationUrl = $"https://ereklamblad.se/{storeOption.Name}?publication={publicationId}",
                Price = offer.Price,
                Savings = offer.Savings,
                RelevantForWeek = relevantForWeek,
            };

            result.Add(scrapedOffer);
        }

        return result;
    }

    private (bool isRelevant, int? relevantForWeek) IngredientIsRelevant(
        string ingredientId,
        Ingredient ingredient,
        Dictionary<int, MealPlanWeek> mealPlan,
        int currentWeek
    )
    {
        if (ingredient.AlwaysRelevant)
            return (true, null);

        int iterations = 0;
        var selectedWeekNumber = currentWeek;
        while (true)
        {
            if (mealPlan.TryGetValue(selectedWeekNumber, out var mealPlanWeek))
            {
                var isInLunch = mealPlanWeek.Lunch.Ingredients.Any(x => x.Contains(ingredientId));
                var isInDinner = mealPlanWeek.Dinner.Ingredients.Any(x => x.Contains(ingredientId));
                if (isInLunch || isInDinner)
                    return (true, selectedWeekNumber);
            }

            iterations++;
            if (!ingredient.MaxWeeksBuyInAdvance.HasValue || iterations > ingredient.MaxWeeksBuyInAdvance)
                return (false, null);

            selectedWeekNumber++;
            if (selectedWeekNumber > 52)
                selectedWeekNumber = 1;
        }
    }
}

