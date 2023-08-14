using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Skender.Stock.Indicators;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Framework.Services;

namespace StockChartsGame.Controllers;

[ApiController]
[Route("")]
public class IndicatorsController : ControllerBase
{
    private readonly IGameService gameService;
    private readonly ChartOptions chartOptions;

    public IndicatorsController(IGameService quotesProvider, IOptions<ChartOptions> chartOptions)
    {
        this.gameService = quotesProvider;
        this.chartOptions = chartOptions.Value;
    }

    [HttpGet("EMA")]
    public IActionResult GetEma(
        int lookbackPeriods)
    {
        try
        {
            IEnumerable<Quote> quotes = gameService.GetQuotes();

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
    public IActionResult GetRsi(
        int lookbackPeriods = 14)
    {
        try
        {
            IEnumerable<Quote> quotes = gameService.GetQuotes();

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
