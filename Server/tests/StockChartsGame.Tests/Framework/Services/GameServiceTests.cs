using Microsoft.Extensions.Options;
using Moq;
using StockChartsGame.Framework.Components;
using StockChartsGame.Framework.Services;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.AlphaVantage.Services;
using StockChartsGame.Providers.Models;
using StockChartsGame.Providers.Series;
using StockChartsGame.Providers.Services;
using SkenderQuote = Skender.Stock.Indicators.Quote;

namespace StockChartsGame.Tests.Framework.Services;

public class GameServiceTests
{
    private readonly string providerSymbol = "TEST";
    private readonly QuoteTimeSeries presetQuoteTimeSeries;
    private readonly Mock<IProvider> providerMock;
    private readonly Mock<IQuoteFrame> quoteFrameMock;
    private readonly Mock<IQuoteFrameFactory> quoteFrameFactoryMock;
    private readonly ChartOptions gameServiceChartOptions;
    private readonly IGameService sut;

    public GameServiceTests()
    {
        this.providerMock = new Mock<IProvider>();
        this.providerMock.Setup(p => p.Symbols).Returns(new string[] { providerSymbol });
        this.providerMock.Setup(p => p.Name).Returns(nameof(AlphaVantageClient));
        var inputQuotes = new List<IQuote>()
        {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(10)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(10))
        };
        this.presetQuoteTimeSeries = new QuoteTimeSeries(inputQuotes, TimeSpan.FromSeconds(10));
        this.providerMock.Setup(p => p.GetTimeSeriesIntradayAsync(providerSymbol)).Returns(Task.FromResult(presetQuoteTimeSeries));

        gameServiceChartOptions = new ChartOptions();
        var gameServiceChartOptionsMock = new Mock<IOptions<ChartOptions>>();
        gameServiceChartOptionsMock.Setup(m => m.Value).Returns(gameServiceChartOptions);

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
        quoteFrameFactoryMock = new Mock<IQuoteFrameFactory>();
        quoteFrameFactoryMock.Setup(m => m.Create(It.IsAny<IProvider>(), It.IsAny<ChartOptions>())).Returns(Task.FromResult(quoteFrameMock.Object));

        this.sut = new GameService(new List<IProvider>() { providerMock.Object }, quoteFrameFactoryMock.Object, gameServiceChartOptionsMock.Object);
    }

    [Fact]
    public void GetQuotes_ValidInput_ExpectedResult()
    {
        var result = sut.GetQuotes();

        result.Should().BeEquivalentTo(quoteFrameMock.Object);
    }

    [Fact]
    public void RevealHiddenQuotes_RevealedSetToTrue()
    {
        sut.RevealHiddenQuotes();

        Assert.True(sut.Revealed);
    }

    [Fact]
    public void RefreshQuotes_FirstCall_CallsCreateOnQuoteFrameFactory()
    {
        sut.RefreshQuotes();

        quoteFrameFactoryMock.Verify(m => m.Create(
            It.Is<IProvider>(p => p == providerMock.Object),
            It.Is<ChartOptions>(o => o == gameServiceChartOptions)),
            Times.Once);
    }

    [Fact]
    public void RefreshQuotes_Revealed_SetsRevealedToFalse()
    {
        sut.RevealHiddenQuotes();
        sut.RefreshQuotes();

        Assert.False(sut.Revealed);
    }

    [Fact]
    public void RefreshQuotes_SecondCall_SingleCallToCreateOnQuoteFrameFactory()
    {
        sut.RefreshQuotes();
        sut.RefreshQuotes();

        quoteFrameFactoryMock.Verify(m => m.Create(
            It.Is<IProvider>(p => p == providerMock.Object),
            It.Is<ChartOptions>(o => o == gameServiceChartOptions)),
            Times.Once);
    }

    [Fact]
    public void RefreshQuotes_SecondCall_CallRefreshOnQuoteFrame()
    {
        sut.RefreshQuotes();
        sut.RefreshQuotes();

        quoteFrameMock.Verify(m => m.Refresh(), Times.Once);
    }
}
