namespace StockChartsGame.Providers.AlphaVantage.Configuration;

public class GameOptions
{
    public const string Section = "Game";

    public int StockExpirySeconds { get; set; } = 20;
}
