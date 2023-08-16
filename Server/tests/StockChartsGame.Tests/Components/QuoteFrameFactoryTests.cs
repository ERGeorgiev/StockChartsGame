using Microsoft.Extensions.Options;
using StockChartsGame.Framework.Components;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.AlphaVantage.Services;
using StockChartsGame.Providers.Models;
using StockChartsGame.Providers.Series;
using StockChartsGame.Providers.Services;

namespace StockChartsGame.Tests.Components;

public class QuoteFrameFactoryTests
{
    private readonly string providerSymbol = "TEST";
    private readonly QuoteTimeSeries presetQuoteTimeSeries;
    private readonly Mock<IProvider> providerMock;
    private readonly Mock<IQuoteFrame> quoteFrameMock;
    private readonly ChartOptions chartOptions;
    private readonly QuoteFrameFactory sut;

    public QuoteFrameFactoryTests()
    {
        this.providerMock = new Mock<IProvider>();
        this.providerMock.Setup(p => p.Symbols).Returns(new string[] { providerSymbol });
        this.providerMock.Setup(p => p.Name).Returns(nameof(AlphaVantageClient));
        var inputQuotes = new List<IQuote>()
        {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(3))
        };
        this.presetQuoteTimeSeries = new QuoteTimeSeries(inputQuotes, TimeSpan.FromSeconds(1));
        this.providerMock.Setup(p => p.GetTimeSeriesIntradayAsync(providerSymbol)).Returns(Task.FromResult(presetQuoteTimeSeries));

        chartOptions = new ChartOptions();
        var gameServiceChartOptionsMock = new Mock<IOptions<ChartOptions>>();
        gameServiceChartOptionsMock.Setup(m => m.Value).Returns(chartOptions);

        quoteFrameMock = new Mock<IQuoteFrame>();
        var quotes = presetQuoteTimeSeries.Select(x => new SkenderQuote()
        {
            Close = Math.Round((decimal)x.Price, 2),
            Date = x.Date,
            High = Math.Round((decimal)x.High, 2),
            Low = Math.Round((decimal)x.Low, 2),
            Open = Math.Round((decimal)x.Open, 2),
            Volume = x.Volume
        });
        quoteFrameMock.Setup(m => m.GetEnumerator()).Returns(quotes.GetEnumerator());

        sut = new QuoteFrameFactory();
    }

    [Fact]
    public async Task Create_SingleDay_ReturnsEquivalentToInput()
    {
        var result = await sut.Create(providerMock.Object, chartOptions);

        result.Should().BeEquivalentTo(quoteFrameMock.Object);
    }

    [Fact]
    public async Task Create_SameSymbolAsProvider()
    {
        var result = await sut.Create(providerMock.Object, chartOptions);

        result.Symbol.Should().BeEquivalentTo(providerSymbol);
    }
}
