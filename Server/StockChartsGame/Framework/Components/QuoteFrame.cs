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
        // ToDo: Handle empty response in the callers
        var days = quotes.Select(q => q.Date.TotalDays()).Distinct().ToArray();
        var availableDays = days.Except(daysQuoted).ToArray();
        IEnumerable<Quote> dayQuotes = new List<Quote>();

        if (availableDays.Any())
        {
            var day = availableDays[rnd.Next(0, availableDays.Length)];
            daysQuoted.Add(day);
            var cutoff = GetCutoffDateTime(day, chartOptions.PriorityHours);
            if (quotes.Any(q => q.Date.TotalDays() == day && q.Date <= cutoff))
            {
                dayQuotes = quotes.Where(q => q.Date <= cutoff);
            }
        }
        if (dayQuotes.Any() == false && days.Any())
        {
            var day = days[rnd.Next(0, days.Length)];
            var cutoff = GetCutoffDateTime(day, chartOptions.FallbackHours);
            if (quotes.Any(q => q.Date.TotalDays() == day && q.Date <= cutoff))
            {
                dayQuotes = quotes.Where(q => q.Date <= cutoff);
            }
        }

        return dayQuotes.OrderBy(q => q.Date);
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

    private DateTime GetCutoffDateTime(int cutoffDay, KeyValuePair<DateTime, DateTime> hours)
    {
        var cutoffDateTime = new DateTime().AddDays(cutoffDay);
        var cutoffHour = RandomDateBetweenDates(hours.Key, hours.Value);
        cutoffDateTime += new TimeSpan(cutoffHour.Hour, cutoffHour.Minute, cutoffHour.Second);

        return cutoffDateTime;
    }

    private DateTime RandomDateBetweenDates(DateTime min, DateTime max)
    {
        TimeSpan timeSpan = max - min;
        TimeSpan span = new(0, rnd.Next(0, (int)timeSpan.TotalMinutes), 0);
        DateTime newDate = min + span;

        return newDate;
    }
}
