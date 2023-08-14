using StockChartsGame.Providers.Series;

namespace StockChartsGame.Providers.Services;

public interface IProvider
{
    string[] Symbols { get; }

    Task<QuoteTimeSeries> GetTimeSeriesIntradayAsync(string symbol);
}
