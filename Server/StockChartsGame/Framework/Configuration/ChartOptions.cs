namespace StockChartsGame.Providers.AlphaVantage.Configuration;

public class ChartOptions
{
    public const string Section = "Chart";

    public int DisplayItemsCount { get; set; } = 400;

    public int HiddenItemsCount { get; set; } = 15;

    public KeyValuePair<DateTime, DateTime> PriorityHours { get; set; } =
        new KeyValuePair<DateTime, DateTime>(DateTime.Parse("9:15"), DateTime.Parse("10:00"));

    public KeyValuePair<DateTime, DateTime> FallbackHours { get; set; } =
        new KeyValuePair<DateTime, DateTime>(DateTime.Parse("12:00"), DateTime.Parse("14:00"));
}
