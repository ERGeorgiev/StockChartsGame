using Quote = Skender.Stock.Indicators.Quote;

namespace StockChartsGame.Framework.Services;

public interface IQuoteFrameService : IOrderedEnumerable<Quote>
{
    string Symbol { get; }
    void Refresh();
}
