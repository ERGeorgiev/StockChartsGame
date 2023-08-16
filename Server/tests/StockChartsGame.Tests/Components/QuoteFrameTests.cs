using Microsoft.Extensions.Options;
using StockChartsGame.Framework.Components;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.AlphaVantage.Services;
using StockChartsGame.Providers.Models;
using StockChartsGame.Providers.Series;
using StockChartsGame.Providers.Services;

namespace StockChartsGame.Tests.Components;

public class QuoteFrameTests
{
    private readonly string providerSymbol = "TEST";
    private readonly Mock<IProvider> providerMock;
    private readonly ChartOptions chartOptions;
    private readonly IEnumerable<SkenderQuote> quotes;
    private QuoteFrame sut;

    public QuoteFrameTests()
    {
        this.providerMock = new Mock<IProvider>();
        this.providerMock.Setup(p => p.Symbols).Returns(new string[] { providerSymbol });
        this.providerMock.Setup(p => p.Name).Returns(nameof(AlphaVantageClient));
        var inputQuotes = new List<IQuote>()
        {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(3)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromDays(1) + TimeSpan.FromSeconds(1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromDays(1) + TimeSpan.FromSeconds(3))
        };
        var quoteTimeSeries = new QuoteTimeSeries(inputQuotes, TimeSpan.FromSeconds(1));
        this.providerMock.Setup(p => p.GetTimeSeriesIntradayAsync(providerSymbol)).Returns(Task.FromResult(quoteTimeSeries));

        chartOptions = new ChartOptions();
        var gameServiceChartOptionsMock = new Mock<IOptions<ChartOptions>>();
        gameServiceChartOptionsMock.Setup(m => m.Value).Returns(chartOptions);

        quotes = quoteTimeSeries.Select(x => new SkenderQuote()
        {
            Close = Math.Round((decimal)x.Price, 2),
            Date = x.Date,
            High = Math.Round((decimal)x.High, 2),
            Low = Math.Round((decimal)x.Low, 2),
            Open = Math.Round((decimal)x.Open, 2),
            Volume = x.Volume
        });
        sut = new QuoteFrame(providerSymbol, quotes, chartOptions);
    }

    [Fact]
    public void Symbol_OnCreate_HasCorrectSymbol()
    {
        Assert.Equal(providerSymbol, sut.Symbol);
    }

    [Fact]
    public void Refresh_TwoDays_LoadsNewFrame()
    {
        var firstFrame = sut.ToArray();
        sut.Refresh();

        var newFrame = sut.ToArray();
        newFrame.Should().NotBeEquivalentTo(firstFrame);
    }

    [Fact]
    public void Enumerator_OneDay_ReturnsEquivalentToInput()
    {
        var inputQuotes = new List<IQuote>()
        {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(2)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(3)),
        };
        var quotes = inputQuotes.Select(x => new SkenderQuote()
        {
            Close = Math.Round((decimal)x.Price, 2),
            Date = x.Date,
            High = Math.Round((decimal)x.High, 2),
            Low = Math.Round((decimal)x.Low, 2),
            Open = Math.Round((decimal)x.Open, 2),
            Volume = x.Volume
        });
        sut = new QuoteFrame(providerSymbol, quotes, chartOptions);

        sut.Should().BeEquivalentTo(quotes);
    }
}
