using Skender.Stock.Indicators;

namespace StockChartsGame.Framework.Services;

public interface IGameService
{
    string? Symbol { get; }
    bool Revealed { get; }
    IEnumerable<Quote> GetQuotes();
    void RefreshQuotes();
    void RevealHiddenQuotes();
}
