using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Skender.Stock.Indicators;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using WebApi.Framework.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("")]
public class IndicatorsController : ControllerBase
{
    private readonly IGameService quotesProvider;
    private readonly ChartOptions chartOptions;

    public IndicatorsController(IGameService quotesProvider, IOptions<ChartOptions> chartOptions)
    {
        this.quotesProvider = quotesProvider;
        this.chartOptions = chartOptions.Value;
    }

    [HttpGet("EMA")]
    public async Task<IActionResult> GetEma(
        int lookbackPeriods)
    {
        try
        {
            IEnumerable<Quote> quotes = await quotesProvider.Get();

            IEnumerable<EmaResult> results =
                quotes.GetEma(lookbackPeriods)
                      .TakeLast(chartOptions.DisplayItemsCount);

            return Ok(results);
        }
        catch (ArgumentOutOfRangeException rex)
        {
            return BadRequest(rex.Message);
        }
    }

    [HttpGet("RSI")]
    public async Task<IActionResult> GetRsi(
        int lookbackPeriods = 14)
    {
        try
        {
            IEnumerable<Quote> quotes = await quotesProvider.Get();

            IEnumerable<RsiResult> results =
                quotes.GetRsi(lookbackPeriods)
                      .TakeLast(chartOptions.DisplayItemsCount);

            return Ok(results);
        }
        catch (ArgumentOutOfRangeException rex)
        {
            return BadRequest(rex.Message);
        }
    }
}
