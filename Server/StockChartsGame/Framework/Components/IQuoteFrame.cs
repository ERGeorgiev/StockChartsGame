using Quote = Skender.Stock.Indicators.Quote;

namespace StockChartsGame.Framework.Components;

public interface IQuoteFrame : IOrderedEnumerable<Quote>
{
    string Symbol { get; }
    void Refresh();
}
