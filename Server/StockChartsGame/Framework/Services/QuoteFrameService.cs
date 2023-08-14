using System.Collections;
using StockChartsGame.Providers.Series;
using StockChartsGame.Providers.Services;
using Quote = Skender.Stock.Indicators.Quote;

namespace StockChartsGame.Framework.Services;

public class QuoteFrameService : IQuoteFrameService
{
    private readonly IEnumerable<Quote> quotes;
    private readonly List<int> daysQuoted = new();
    private readonly Random rnd = new();
    private IOrderedEnumerable<Quote> quoteFrame;

    private QuoteFrameService(string symbol, IEnumerable<Quote> quotes)
    {
        this.Symbol = symbol;
        this.quotes = quotes;
        this.quoteFrame = GetQuoteFrame();
    }
    public string Symbol { get; private set; }

    public static async Task<QuoteFrameService> Create(IProvider provider)
    {
        var symbol = provider.Symbols[new Random().Next(0, provider.Symbols.Length)];
        var quotes = await Fetch(provider, symbol);

        return new QuoteFrameService(symbol, quotes);
    }

    public void Refresh()
    {
        quoteFrame = GetQuoteFrame();
    }

    public IOrderedEnumerable<Quote> CreateOrderedEnumerable<TKey>(Func<Quote, TKey> keySelector, IComparer<TKey>? comparer, bool descending)
    {
        return quoteFrame.CreateOrderedEnumerable(keySelector, comparer, descending);
    }

    public IEnumerator<Quote> GetEnumerator()
    {
        return quoteFrame.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)quoteFrame).GetEnumerator();
    }

    private IOrderedEnumerable<Quote> GetQuoteFrame()
    {
        var bufferDays = 2;
        var days = quotes.Select(q => q.Date.DayOfYear).Distinct().ToList();
        var availableDays = days.Except(daysQuoted).ToArray();
        List<Quote> dayQuotes;
        if (availableDays.Length > bufferDays)
        {
            var dayIndex = rnd.Next(bufferDays, availableDays.Length);
            var day = availableDays[dayIndex];
            daysQuoted.Add(day);

            var endMinute = rnd.Next(31, 45) + 15;
            dayQuotes = quotes.Where(q => q.Date.DayOfYear == day && q.Date.Hour == 9 && q.Date.Minute <= endMinute).ToList();
        }
        else
        {
            var dayIndex = rnd.Next(bufferDays, days.Count);
            var day = days[dayIndex];

            dayQuotes = quotes.Where(q => q.Date.DayOfYear == day && q.Date.Hour >= 10 && q.Date.Hour < 14).ToList();
        }

        var dayQuotesStart = dayQuotes.Min(q => q.Date);
        var finalQuotes = quotes.Where(q => q.Date < dayQuotesStart).Concat(dayQuotes).OrderBy(x => x.Date);

        return finalQuotes;
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
