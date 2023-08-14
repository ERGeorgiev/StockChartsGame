using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Skender.Stock.Indicators;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using WebApi.Framework.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("")]
public class GameController : ControllerBase
{
    private readonly IGameService quotesProvider;
    private readonly ChartOptions options;

    public GameController(IGameService quotesProvider, IOptions<ChartOptions> options)
    {
        this.quotesProvider = quotesProvider;
        this.options = options.Value;
    }

    [HttpPost("refreshQuotes")]
    public async Task<IActionResult> RefreshQuotes()
    {
        await quotesProvider.Refresh();
        return Ok();
    }

    [HttpPost("reveal")]
    public async Task<IActionResult> Reveal()
    {
        await quotesProvider.Reveal();
        return Ok();
    }

    [HttpGet("symbol")]
    public IActionResult GetSymbol()
    {
        return Ok(JsonConvert.SerializeObject(quotesProvider.Stock));
    }

    [HttpGet("quotes")]
    public async Task<IActionResult> GetQuotes()
    {
        IEnumerable<Quote> quotes = await quotesProvider.Get();

        return Ok(quotes.TakeLast(options.DisplayItemsCount));
    }
}
