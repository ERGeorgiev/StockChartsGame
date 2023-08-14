using Microsoft.Extensions.Options;
using Moq;
using StockChartsGame.Framework.Services;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.Models;
using StockChartsGame.Providers.Series;
using StockChartsGame.Providers.Services;

namespace StockChartsGame.Tests.Framework.Services;

public class GameServiceTests
{
    private readonly string providerSymbol = "TEST";
    private readonly Mock<IProvider> providerMock;
    private readonly ChartOptions gameServiceChartOptions;
    private readonly IGameService sut;

    public GameServiceTests()
    {
        providerMock = new Mock<IProvider>();
        providerMock.Setup(p => p.Symbols).Returns(new string[] { providerSymbol });
        var inputQuotes = new List<IQuote>()
        {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(10))
        };
        var getTimeSeriesIntradayAsyncPresetResult = new QuoteTimeSeries(inputQuotes, TimeSpan.FromSeconds(10));
        providerMock.Setup(p => p.GetTimeSeriesIntradayAsync(providerSymbol)).Returns(Task.FromResult(getTimeSeriesIntradayAsyncPresetResult));

        gameServiceChartOptions = new ChartOptions();
        var gameServiceChartOptionsMock = new Mock<IOptions<ChartOptions>>();
        gameServiceChartOptionsMock.Setup(m => m.Value).Returns(gameServiceChartOptions);

        sut = new GameService(new List<IProvider>() { providerMock.Object }, gameServiceChartOptionsMock.Object);
    }

    [Fact]
    public void Refresh_ValidInput_ExpectedResult()
    {
        
    }

    [Fact]
    public void Reveal_ValidInput_ExpectedResult()
    {
    }
}
