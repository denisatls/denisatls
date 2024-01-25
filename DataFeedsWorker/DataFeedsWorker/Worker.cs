using DataFeedsWorker.Data;
using DataFeedsWorker.Services;

namespace DataFeedsWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly DbListener _listener;
    private Task _listeningTask;
    private Procedures _dbProcedures;

    public Worker(ILogger<Worker> logger, DbListener listener, Procedures dbProcedures)
    {
        _logger = logger;
        _listener = listener;
        _dbProcedures = dbProcedures;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start listening for insert notifications
        _listeningTask = Task.Run(() => _listener.StartListeningForInserts(), stoppingToken);

        _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
        
        // Wait for the listening task to complete when the worker is stopping
        await _listeningTask;
        
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