namespace StockChartsGame.Providers.AlphaVantage.Configuration;

public class AlphaVantageOptions
{
    public const string Section = "Providers:AlphaVantage";

    public string? ApiKey { get; set; }

    public string[] Symbols { get; set; } = Array.Empty<string>();
}
