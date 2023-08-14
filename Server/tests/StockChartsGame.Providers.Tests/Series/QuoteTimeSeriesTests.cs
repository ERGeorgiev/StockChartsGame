using StockChartsGame.Providers.Models;
using StockChartsGame.Providers.Series;

namespace StockChartsGame.Providers.Tests.Series;

public class QuoteTimeSeriesTests
{
    [Fact]
    public void Constructor_InputWithGaps_FillsGaps()
    {
        var period = TimeSpan.FromSeconds(1);
        var input = new List<IQuote>() {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 4)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 9)),
        };
        var expectedResult = new List<IQuote>() {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 2)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 3)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 4)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 5)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 6)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 7)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 8)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 9)),
        };

        var result = new QuoteTimeSeries(input, period);

        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void Constructor_InputWithGaps_DoesNotFillMissingDays()
    {
        var period = TimeSpan.FromSeconds(1);
        var input = new List<IQuote>() {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 4)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 9)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromDays(10)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromDays(10) + (period * 3)),
        };
        var expectedResult = new List<IQuote>() {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 2)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 3)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 4)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 5)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 6)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 7)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 8)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + (period * 9)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromDays(10)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromDays(10) + (period * 1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromDays(10) + (period * 2)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromDays(10) + (period * 3)),
        };

        var result = new QuoteTimeSeries(input, period);

        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void Constructor_InputWithGapsUnevenPeriod_FillsGaps()
    {
        var period = TimeSpan.FromSeconds(2);
        var input = new List<IQuote>() {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(10)),
        };
        var expectedResult = new List<IQuote>() {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(1)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(3)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(5)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(7)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(9)),
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue + TimeSpan.FromSeconds(10)),
        };

        var result = new QuoteTimeSeries(input, period);

        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void Constructor_SingleItemInput_EquivalentItemResult()
    {
        var period = TimeSpan.FromSeconds(1);
        var input = new List<IQuote>() {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue),
        };
        var expectedResult = new List<IQuote>() {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue),
        };

        var result = new QuoteTimeSeries(input, period);

        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void Constructor_EmptyInput_EmptyResult()
    {
        var period = TimeSpan.FromSeconds(1);
        var input = new List<IQuote>();
        var expectedResult = new List<IQuote>();

        var result = new QuoteTimeSeries(input, period);

        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void Constructor_NullInput_ThrowsNullReferenceException()
    {
        var period = TimeSpan.FromSeconds(1);

        Assert.Throws<ArgumentNullException>(() => new QuoteTimeSeries(null!, period));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidPeriod_ThrowsArgumentOutOfRangeException(double value)
    {
        var period = TimeSpan.FromSeconds(value);

        var input = new List<IQuote>() {
            new Quote(1, 1, 1, 1, 1, DateTime.MinValue),
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => new QuoteTimeSeries(input, period));
    }
}
