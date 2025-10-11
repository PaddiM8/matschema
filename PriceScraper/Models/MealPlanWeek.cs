namespace PriceScraper.Models;

public class MealPlanWeek
{
    public required MealPlanItem Lunch { get; init; }

    public required MealPlanItem Dinner { get; init; }
}
