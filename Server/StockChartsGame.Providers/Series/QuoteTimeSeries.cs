using System.Collections;
using StockChartsGame.Providers.Models;

namespace StockChartsGame.Providers.Series;

public class QuoteTimeSeries : IOrderedEnumerable<IQuote>
{
    private readonly IOrderedEnumerable<IQuote> items;

    public QuoteTimeSeries(IEnumerable<IQuote> items, TimeSpan period)
    {
        this.items = FillPeriodGaps(items, period);
    }

    private static IOrderedEnumerable<IQuote> FillPeriodGaps(IEnumerable<IQuote> items, TimeSpan period)
    {
        if (period <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period));

        var quotes = new List<IQuote>();

        foreach (var dayItems in items.GroupBy(x => (int)(x.Date - DateTime.MinValue).TotalDays))
        {
            var sortedItems = dayItems.OrderBy(i => i.Date).ToArray();

            for (int i = 0; i < sortedItems.Length; i++)
            {
                quotes.Add(sortedItems[i]);
                if (i + 1 < sortedItems.Length)
                {
                    while (sortedItems[i + 1].Date > quotes.Last().Date + period)
                    {
                        var newItem = new Quote(
                            sortedItems[i].Price,
                            sortedItems[i].Price,
                            sortedItems[i].Price,
                            sortedItems[i].Price,
                            sortedItems[i].Volume,
                            quotes.Last().Date + period);
                        quotes.Add(newItem);
                    }
                }
            }
        }

        return quotes.OrderBy(x => x.Date);
    }

    public IOrderedEnumerable<IQuote> CreateOrderedEnumerable<TKey>(Func<IQuote, TKey> keySelector, IComparer<TKey>? comparer, bool descending)
    {
        return items.CreateOrderedEnumerable(keySelector, comparer, descending);
    }

    public IEnumerator<IQuote> GetEnumerator()
    {
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)items).GetEnumerator();
    }
}
