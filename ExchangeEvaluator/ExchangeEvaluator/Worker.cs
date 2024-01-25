using ExchangeEvaluator.Services;

namespace ExchangeEvaluator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ExchangeProcessingService _exchangeProcessingService;
    private readonly MetricsManager _metricsManager; 

    public Worker(ILogger<Worker> logger, ExchangeProcessingService exchangeProcessingService, MetricsManager metricsManager)
    {
        _logger = logger;
        _exchangeProcessingService = exchangeProcessingService;
        _metricsManager = metricsManager; 
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
            try
            {
                await _exchangeProcessingService.ProcessExchangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing exchanges");
                _metricsManager.IncrementGeneralEx(); 
            }
            await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
        }
    }
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ExchangeEvaluator Service started at: {Time}", DateTimeOffset.Now);
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ExchangeEvaluator Service stopped at: {Time}", DateTimeOffset.Now);
        await base.StopAsync(cancellationToken);
    }
}