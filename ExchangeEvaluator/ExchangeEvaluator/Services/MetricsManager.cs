using Prometheus;

namespace ExchangeEvaluator.Services;

public class MetricsManager
{
    private Counter? ErrorMetrics { get; set; }

    public MetricsManager(ILogger<MetricsManager> logger)
    {
        var metricServer = new MetricServer(port: 9915);
        metricServer.Start();
        logger.LogInformation( "Prometheus started : MetricServer(port: 9915)");
        ErrorMetrics = Metrics.CreateCounter("data_feed_worker_exception","Exception in DataFeedWorker");
    }
    
    public void IncrementGeneralEx()
    { 
        ErrorMetrics?.Inc();
    } 
}