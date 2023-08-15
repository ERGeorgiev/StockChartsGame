using Microsoft.Extensions.Options;
using Moq;
using StockChartsGame.Framework.Services;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.AlphaVantage.Services;
using StockChartsGame.Providers.Models;
using StockChartsGame.Providers.Series;
using StockChartsGame.Providers.Services;

namespace StockChartsGame.Tests.Framework.Services;

public class GameServiceTests
{
    private readonly string providerSymbol = "TEST";
    private readonly QuoteTimeSeries presetQuoteTimeSeries;
    private readonly Mock<IProvider> providerMock;
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

        this.sut = new GameService(new List<IProvider>() { providerMock.Object }, gameServiceChartOptionsMock.Object);
    }

    [Fact]
    public void GetQuotes_ValidInput_ExpectedResult()
    {
        //var result = sut.GetQuotes();

        //result.Should().BeEquivalentTo(presetQuoteTimeSeries);
    }

    [Fact]
    public void RefreshQuotes_ValidInput_ExpectedResult()
    {
    }

    [Fact]
    public void RevealHiddenQuotes_ValidInput_ExpectedResult()
    {
    }
}
