using Ardalis.GuardClauses;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.AlphaVantage.Models;
using StockChartsGame.Providers.Models;
using StockChartsGame.Providers.Series;
using StockChartsGame.Providers.Services;

namespace StockChartsGame.Providers.AlphaVantage.Services;

public class AlphaVantageClient : Provider
{
    private const string domain = "https://www.alphavantage.co";
    private const string intervalString = "1min";
    private static readonly TimeSpan interval = TimeSpan.FromMinutes(1);
    private readonly ILogger<AlphaVantageClient> logger;
    private readonly AlphaVantageOptions options;

    public AlphaVantageClient(IOptions<AlphaVantageOptions> options, ILogger<AlphaVantageClient> logger)
    {
        Guard.Against.NullOrEmpty(options.Value.ApiKey, nameof(options.Value.ApiKey));
        this.options = options.Value;
        this.logger = logger;
    }

    public override string[] Symbols => options.Symbols;

    public override async Task<QuoteTimeSeries> GetTimeSeriesIntradayAsync(string symbol)
    {
        var param = new Dictionary<string, string?>()
        {
            { "function", "TIME_SERIES_INTRADAY" },
            { "symbol", symbol },
            { "apikey", options.ApiKey },
            { "interval", intervalString },
            { "outputsize", "full" }
        };

        try
        {
            var timeSeriesItems = await GetTimeSeriesAsync<TimeSeriesIntraday>(param);
            QuoteTimeSeries timeSeries = new(timeSeriesItems, interval);
            return timeSeries;
        }
        catch (Exception e)
        {
            logger.LogError("Failed to acquire data. Error: {Error}", e.Message);
            throw;
        }
    }

    private async Task<IEnumerable<T>> GetTimeSeriesAsync<T>(Dictionary<string, string?> param)
        where T : IQuote
    {
        var queryResult = await QueryAsync(param);
        var jObjectName = $"Time Series ({param["interval"]})";
        var timeSeriesSourceToken = Guard.Against.Null(queryResult[jObjectName], jObjectName);

        var timeSeriesItems = new List<T>();
        var timeSeriesSource = timeSeriesSourceToken.Reverse().ToList();
        foreach (JToken resultItem in timeSeriesSource)
        {
            var jProperty = Guard.Against.Null(resultItem.ToObject<JProperty>(), nameof(resultItem));
            var values = Guard.Against.Null(jProperty.First, nameof(jProperty.First));
            string name = jProperty.Name;
            DateTime date = DateTime.Parse(name);
            var timeSeriesItem = Guard.Against.Null(values.ToObject<T>(), nameof(values));
            timeSeriesItem.Date = date;
            timeSeriesItems.Add(timeSeriesItem);
        }

        return timeSeriesItems;
    }

    private async Task<JObject> QueryAsync(Dictionary<string, string?> param)
    {
        string url = domain + "/query";

        string data;
        var newUrl = new Uri(QueryHelpers.AddQueryString(url, param));
        logger.LogDebug("Querying uri '{Uri}'", JsonConvert.SerializeObject(newUrl));
        var query = await QueryAsync<string>(newUrl);
        data = await query.ReadAsStringAsync();
        if (string.IsNullOrEmpty(data)) throw new Exception("Received empty response.");

        var jObj = JObject.Parse(data);
        if (jObj == null || jObj.HasValues == false)
            throw new Exception($"No valid data found in: '{data}'");
        if (jObj.ContainsKey("Error Message"))
        {
            var errorMessage = jObj["Error Message"]!;
            throw new Exception(errorMessage.ToString());
        }

        return jObj;
    }
}
