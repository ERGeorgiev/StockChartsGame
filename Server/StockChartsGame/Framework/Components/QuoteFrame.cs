using System.Collections;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.Series;
using StockChartsGame.Providers.Services;
using Quote = Skender.Stock.Indicators.Quote;

namespace StockChartsGame.Framework.Components;

public class QuoteFrame : IQuoteFrame
{
    private readonly IEnumerable<Quote> quotes;
    private readonly ChartOptions chartOptions;

    private readonly List<int> daysQuoted = new();
    private readonly Random rnd = new();
    private IOrderedEnumerable<Quote> quoteFrame;

    private QuoteFrame(string symbol, IEnumerable<Quote> quotes, ChartOptions chartOptions)
    {
        this.Symbol = symbol;
        this.quotes = quotes;
        this.chartOptions = chartOptions;
        this.quoteFrame = GetQuoteFrame();
    }
    public string Symbol { get; private set; }

    public static async Task<QuoteFrame> Create(IProvider provider, ChartOptions chartOptions)
    {
        var symbol = provider.Symbols[new Random().Next(0, provider.Symbols.Length)];
        var quotes = await Fetch(provider, symbol);

        return new QuoteFrame(symbol, quotes, chartOptions);
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
        // ToDo: Automatic buffer quotes based on Period (for example how quotes before the cutoff one to give)
        // ToDo: Configurable priority (for example only aim at 9 to 12)
        // ToDo: Configurable fill hours (for example 12 to EOD when priority are used up)
        // ToDo: Automatic buffer between the hours (for example if 9:15 was returned, don't return 9:16 next)
        // ToDo: Handle empty response in the callers
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
            // ToDo: Revisit
            var dayIndex = rnd.Next(0, availableDays.Length);
            var day = availableDays[dayIndex];

            dayQuotes = quotes.Where(q => q.Date.DayOfYear == day && q.Date.Hour >= 10 && q.Date.Hour < 14).ToList();
        }

        if (dayQuotes.Any() == false) return new List<Quote>().OrderBy(q => q);

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
