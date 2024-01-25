
using Serilog;
using ExchangeEvaluator.Data;
using ExchangeEvaluator.Services;
using ExchangeEvaluator;
using ExchangeEvaluator.Discord;
using ExchangeEvaluator.Models;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((hostContext, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(hostContext.Configuration) // Read Serilog configuration
        .Enrich.FromLogContext()
    )
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        
        // Bind the DiscordSettings section
        var discordSettings = new DiscordSettings();
        configuration.GetSection("DiscordSettings").Bind(discordSettings);
        
        // Register IHttpClientFactory
        services.AddHttpClient();

        // Register other services as singletons
        services.AddSingleton<ExchangeService>();
        services.AddSingleton<Procedures>(provider => new Procedures(configuration));
        services.AddSingleton<ExchangeProcessingService>();
        services.AddSingleton<CoinEvaluator>();
        services.AddSingleton<MetricsManager>();
        
        // Register DiscordWebhookClient as a singleton
        services.AddSingleton<DiscordWebhookClient>(provider =>
            new DiscordWebhookClient(discordSettings.WebhookUrl, provider.GetRequiredService<ILogger<DiscordWebhookClient>>()));


        // Add the Worker as a hosted service
        services.AddHostedService<Worker>();
    })
    .Build();

// Ensure any buffered events are written to the log before the application shuts down
AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();

host.Run();