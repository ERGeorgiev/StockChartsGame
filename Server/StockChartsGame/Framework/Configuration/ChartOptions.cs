namespace StockChartsGame.Providers.AlphaVantage.Configuration;

public class ChartOptions
{
    public const string Section = "Chart";

    public int DisplayItemsCount { get; set; } = 400;

    public int HiddenItemsCount { get; set; } = 15;
}
