using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.Services;

namespace StockChartsGame.Framework.Components;

public interface IQuoteFrameFactory
{
    Task<IQuoteFrame> Create(IProvider provider, ChartOptions chartOptions);
}
