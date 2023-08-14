using Skender.Stock.Indicators;

namespace WebApi.Framework.Services;

public interface IGameService
{
    string Stock { get; }
    Task<IEnumerable<Quote>> Get();
    Task Refresh();
    Task Reveal();
}
