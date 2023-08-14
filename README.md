# Stock Charts Game

This is a game based on stock charts. Your goal is to guess if the stock price is going to go up or down, depending on the historical movement and the provided indicators.  

![Demo](./Client/src/assets/Demo.gif)

This project uses Angular, [Chart.js](https://github.com/chartjs/chartjs-chart-financial) financial/candlestick stock chart, with a .NET Web API backend, which connects to [AlphaVantage](https://www.alphavantage.co/documentation/) for stock quotes.  

Based on the [Stock.Charts](https://www.github.com/DaveSkender/Stock.Charts) project.  

## Running this demo locally

If you want to host on your local computer and review the source code, follow the instructions below.

### Prerequisites

- [Node.js](https://nodejs.org/) v14.x or later
- [Visual Studio](http://visualstudio.com)

### Steps

1. Get a free [api key](https://www.alphavantage.co/support/#api-key) from AlphaVantage.
1. Input your api key into appsettings.json - `Providers:AlphaVantage:ApiKey`.
1. Load the WebApi project in Visual Studio and start. That should also run the angular front-end and open the main page for you.

### Attributions

[Stocks](https://icons8.com/icon/NwAwrEVExFBt/stocks) icon by [Icons8](https://icons8.com)

### ToDo
- Finish Unit Tests
- Angular Unit Tests
- Support for Multiple Users
- Better way to provide api key
