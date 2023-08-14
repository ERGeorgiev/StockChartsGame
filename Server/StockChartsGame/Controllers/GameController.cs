using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Skender.Stock.Indicators;
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Framework.Services;

namespace StockChartsGame.Controllers;

[ApiController]
[Route("")]
public class GameController : ControllerBase
{
    private readonly IGameService gameService;
    private readonly ChartOptions options;

    public GameController(IGameService gameService, IOptions<ChartOptions> options)
    {
        this.gameService = gameService;
        this.options = options.Value;
    }

    [HttpGet("symbol")]
    public IActionResult GetSymbol()
    {
        return Ok(JsonConvert.SerializeObject(gameService.Symbol));
    }

    [HttpGet("quotes")]
    public IActionResult GetQuotes()
    {
        IEnumerable<Quote> quotes = gameService.GetQuotes();

        return Ok(quotes.TakeLast(options.DisplayItemsCount));
    }

    [HttpPost("reveal")]
    public IActionResult Reveal()
    {
        gameService.RevealHiddenQuotes();
        return Ok();
    }

    [HttpPost("refreshQuotes")]
    public IActionResult RefreshQuotes()
    {
        gameService.RefreshQuotes();
        return Ok();
    }
}
