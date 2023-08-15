// STARTUP CONFIGURATION
using StockChartsGame.Providers.AlphaVantage.Configuration;
using StockChartsGame.Providers.AlphaVantage.Services;
using StockChartsGame.Providers.Services;
using StockChartsGame.Framework.Services;
using StockChartsGame.Framework.Components;

Environment.SetEnvironmentVariable("Browser", "none");
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
ConfigurationManager configuration = builder.Configuration;

// add framework services
services.AddControllers()
        .AddNewtonsoftJson(x =>
           x.SerializerSettings.ReferenceLoopHandling
           = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// setup CORS for website
IConfigurationSection corsOrigins = configuration.GetSection("CorsOrigins");

services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
    cors =>
    {
        cors.AllowAnyHeader();
        cors.AllowAnyMethod();
        cors.WithOrigins(corsOrigins["Website"]);
    });
});

// WebApi
services.Configure<ChartOptions>(configuration.GetSection(ChartOptions.Section));
services.Configure<GameOptions>(configuration.GetSection(GameOptions.Section));

// Providers
services.Configure<AlphaVantageOptions>(configuration.GetSection(AlphaVantageOptions.Section));

// Main
services.AddSingleton<IQuoteFrameFactory, QuoteFrameFactory>();
services.AddSingleton<IProvider, AlphaVantageClient>();
services.AddSingleton<IGameService, GameService>();

Console.WriteLine($"CORS Origins: {corsOrigins["Website"]}");

// build application
WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
_ = app.Environment.IsDevelopment()
  ? app.UseDeveloperExceptionPage()
  : app.UseHsts();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseResponseCaching();
app.MapControllers();
app.Run();
