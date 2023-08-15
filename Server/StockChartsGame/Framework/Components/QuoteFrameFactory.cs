using System.Collections;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.Series;
using StockChartsGame.Providers.Services;
using Quote = Skender.Stock.Indicators.Quote;

namespace StockChartsGame.Framework.Components;

public class QuoteFrameFactory : IQuoteFrameFactory
{
    private readonly Random rnd = new();

    public async Task<IQuoteFrame> Create(IProvider provider, ChartOptions chartOptions)
    {
        var symbol = provider.Symbols[rnd.Next(0, provider.Symbols.Length)];
        var quotes = await Fetch(provider, symbol);

        return new QuoteFrame(symbol, quotes, chartOptions);
    }

    private static async Task<IEnumerable<Quote>> Fetch(IProvider provider, string symbol)
    {
        QuoteTimeSeries timeSeries = await provider.GetTimeSeriesIntradayAsync(symbol);
        var quotes = timeSeries.Select(x => new Quote()
        {
            Close = Math.Round((decimal)x.Price, 2),
            Date = x.Date,
            High = Math.Round((decimal)x.High, 2),
            Low = Math.Round((decimal)x.Low, 2),
            Open = Math.Round((decimal)x.Open, 2),
            Volume = x.Volume
        });

        return quotes;
    }
}
