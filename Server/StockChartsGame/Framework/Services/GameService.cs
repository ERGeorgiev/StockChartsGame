using System.Diagnostics;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using StockChartsGame.Framework.Components;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.AlphaVantage.Services;
using StockChartsGame.Providers.Services;
using Quote = Skender.Stock.Indicators.Quote;

namespace StockChartsGame.Framework.Services;

public class GameService : IGameService
{
    private readonly IProvider provider;
    private readonly IQuoteFrameFactory quoteFrameFactory;
    private readonly ChartOptions chartOptions;

    private readonly object quoteFrameLock = new();
    private readonly Stopwatch lastFetch = new();
    private readonly Random rnd = new();
    private IQuoteFrame? quoteFrame;

    public GameService(IEnumerable<IProvider> providers, IQuoteFrameFactory quoteFrameFactory, IOptions<ChartOptions> chartOptions)
    {
        this.provider = providers.Single(p => p.Name == nameof(AlphaVantageClient));
        this.quoteFrameFactory = quoteFrameFactory;
        this.chartOptions = chartOptions.Value;
    }

    public string? Symbol => quoteFrame?.Symbol;
    public bool Revealed { get; private set; } = false;

    public IEnumerable<Quote> GetQuotes()
    {
        lock (quoteFrameLock)
        {
            if (quoteFrame == null) RefreshQuotes();
            Guard.Against.NullOrEmpty(quoteFrame, nameof(quoteFrame));

            if (Revealed) return quoteFrame;

            return quoteFrame.Take(quoteFrame.Count() - chartOptions.HiddenItemsCount);
        }
    }

    public void RevealHiddenQuotes()
    {
        Revealed = true;
    }

    public void RefreshQuotes()
    {
        lock (quoteFrameLock)
        {
            Revealed = false;
            if (quoteFrame == null || lastFetch.IsRunning == false || lastFetch.Elapsed > TimeSpan.FromSeconds(20))
            {
                quoteFrame = quoteFrameFactory.Create(provider, chartOptions).Result;
                lastFetch.Restart();
            }
            else
            {
                quoteFrame.Refresh();
            }
        }
    }
}
