namespace StockChartsGame.Providers.Models;

public interface IQuote
{
    float Open { get; set; }

    float High { get; set; }

    float Low { get; set; }

    float Price { get; set; }

    int Volume { get; set; }

    DateTime Date { get; set; }
}
