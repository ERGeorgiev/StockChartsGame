namespace StockChartsGame.Providers.Models;

public class Quote : IQuote
{
    public Quote(float open, float high, float low, float price, int volume, DateTime date)
    {
        Open = open;
        High = high;
        Low = low;
        Price = price;
        Volume = volume;
        Date = date;
    }

    public float Open { get; set; }

    public float High { get; set; }

    public float Low { get; set; }

    public float Price { get; set; }

    public int Volume { get; set; }

    public DateTime Date { get; set; }
}
