using System.Net.Http.Headers;
using StockChartsGame.Providers.Series;

namespace StockChartsGame.Providers.Services;

public abstract class Provider : IProvider
{
    public abstract string[] Symbols { get; }

    public static async Task<HttpContent> QueryAsync<T>(Uri uri)
    {
        using HttpClient client = new()
        {
            BaseAddress = uri
        };

        client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response = await client.GetAsync(uri.Query);
        if (response.IsSuccessStatusCode)
            return response.Content;
        else
            throw new HttpRequestException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
    }

    public abstract Task<QuoteTimeSeries> GetTimeSeriesIntradayAsync(string symbol);
}
