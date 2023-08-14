using System.Diagnostics;
using Microsoft.Extensions.Options;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.AlphaVantage.Services;
using StockChartsGame.Providers.Series;
using StockChartsGame.Providers.Services;
using Quote = Skender.Stock.Indicators.Quote;

namespace WebApi.Framework.Services;

internal class GameService : IGameService
{
    private readonly IProvider provider;
    private IEnumerable<Quote> cachedQuotes;
    private IEnumerable<Quote> quotes;
    public bool Revealed = false;
    private readonly List<int> daysQuoted = new();
    private readonly Stopwatch lastFetch = new();
    private readonly Random rnd = new();
    private readonly ChartOptions chartOptions;

    public GameService(IEnumerable<IProvider> providers, IOptions<ChartOptions> chartOptions)
    {
        provider = providers.Single(p => p.GetType().Name == nameof(AlphaVantageClient));
        this.chartOptions = chartOptions.Value;
    }

    public string Stock { get; private set; }

    public async Task<IEnumerable<Quote>> Get()
    {
        if (quotes == null)
        {
            await Refresh();
        }

        if (Revealed == false)
        {
            return quotes?.Take(quotes.Count() - 15);
        }

        return quotes;
    }

    public async Task Refresh()
    {
        await Fetch();
        Revealed = false;
        quotes = GetRandomTimeFrame();
    }

    public Task Reveal()
    {
        Revealed = true;

        return Task.CompletedTask;
    }

    private IEnumerable<Quote> GetRandomTimeFrame()
    {
        var bufferDays = 2;
        var days = cachedQuotes.Select(q => q.Date.DayOfYear).Distinct().ToList();
        var availableDays = days.Except(daysQuoted).ToArray();
        List<Quote> dayQuotes;
        if (availableDays.Length > bufferDays)
        {
            var dayIndex = rnd.Next(bufferDays, availableDays.Length);
            var day = availableDays[dayIndex];
            daysQuoted.Add(day);

            var endMinute = rnd.Next(31, 45) + 15;
            dayQuotes = cachedQuotes.Where(q => q.Date.DayOfYear == day && q.Date.Hour == 9 && q.Date.Minute <= endMinute).ToList();
        }
        else
        {
            var dayIndex = rnd.Next(bufferDays, days.Count);
            var day = days[dayIndex];

            dayQuotes = cachedQuotes.Where(q => q.Date.DayOfYear == day && q.Date.Hour >= 10 && q.Date.Hour < 14).ToList();
        }

        var dayQuotesStart = dayQuotes.Min(q => q.Date);
        var finalQuotes = cachedQuotes.Where(q => q.Date < dayQuotesStart);
        finalQuotes = finalQuotes.Concat(dayQuotes);
        finalQuotes = finalQuotes.OrderBy(x => x.Date).ToList();

        return finalQuotes;
    }

    private async Task Fetch()
    {
        if (lastFetch.IsRunning == false || lastFetch.Elapsed > TimeSpan.FromSeconds(20))
        {
            daysQuoted.Clear();
            Stock = provider.Symbols[rnd.Next(provider.Symbols.Length)];
            QuoteTimeSeries timeSeries = await provider.GetTimeSeriesIntradayAsync(Stock);
            cachedQuotes = timeSeries.Select(x => new Quote()
            {
                Close = Math.Round((decimal)x.Price, 2),
                Date = x.Date,
                High = Math.Round((decimal)x.High, 2),
                Low = Math.Round((decimal)x.Low, 2),
                Open = Math.Round((decimal)x.Open, 2),
                Volume = x.Volume
            });
            lastFetch.Restart();
        }
    }
}
