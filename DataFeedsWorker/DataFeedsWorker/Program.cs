using DataFeedsWorker;
using DataFeedsWorker.Data;
using DataFeedsWorker.Services;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(app =>
    {
        app.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(hostContext.Configuration)
            .CreateLogger();

        // Add Worker service to the DI container
        services.AddHostedService<Worker>();

        // Add TmuxService and YamlGenerator to DI container
        services.AddSingleton<TmuxService>();
        services.AddSingleton<YamlGenerator>();

        // Configure TmuxService and YamlGenerator settings
        var tmuxConfig = hostContext.Configuration.GetSection("Tmux").Get<TmuxConfig>();
        var yamlFileConfig = hostContext.Configuration.GetSection("YamlFile").Get<YamlFileConfig>();
        services.AddSingleton(tmuxConfig);
        services.AddSingleton(yamlFileConfig);
        services.AddSingleton<DbListener>();
        services.AddSingleton<Procedures>();
        services.AddSingleton<MetricsManager>();

    })
    .UseSerilog() // Use Serilog for logging
    .Build();

host.Run(); //yo