using Newtonsoft.Json;
using StockChartsGame.Providers.Models;

namespace StockChartsGame.Providers.AlphaVantage.Models;

public class TimeSeriesIntraday : IQuote
{
    public DateTime Date { get; set; }

    [JsonProperty("1. open")]
    public float Open { get; set; }

    [JsonProperty("2. high")]
    public float High { get; set; }

    [JsonProperty("3. low")]
    public float Low { get; set; }

    [JsonProperty("4. close")]
    public float Price { get; set; }

    [JsonProperty("5. volume")]
    public int Volume { get; set; }
}
