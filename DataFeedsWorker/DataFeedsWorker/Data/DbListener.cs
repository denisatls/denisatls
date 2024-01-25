using Npgsql;
using DataFeedsWorker.Services;

namespace DataFeedsWorker.Data
{
    public class DbListener
    {
        private readonly string? _connectionString;
        private readonly ILogger<DbListener> _logger;
        private readonly Procedures _dbProc;
        private readonly YamlGenerator _ymGenerator;
        private readonly YamlFileConfig _yamlFileConfig;
        private readonly TmuxService _tmuxService;
        private readonly MetricsManager _metricsManager;

        public DbListener(IConfiguration configuration, ILogger<DbListener> logger, Procedures dbProc, YamlGenerator ymGenerator, YamlFileConfig yamlFileConfig, TmuxService tmuxService, MetricsManager metricsManager)
        {
            _connectionString = configuration.GetConnectionString("ExchangeEvaluatorDB");
            _logger = logger;
            _dbProc = dbProc;
            _ymGenerator = ymGenerator;
            _yamlFileConfig = yamlFileConfig;
            _tmuxService = tmuxService;
            _metricsManager = metricsManager;
        }

        public void StartListeningForInserts()
        {
            Task.Run(async () => 
            {
                try
                {
                    using var conn = new NpgsqlConnection(_connectionString);
                        conn.Open();

                    using (var cmd = new NpgsqlCommand("LISTEN insert_notification;", conn))
                        cmd.ExecuteNonQuery();


                    _logger.LogInformation("Waiting for notifications...");
                    conn.Notification += async (o, e) => 
                    {
                        _logger.LogInformation("Row with ID {ID} was inserted", e.Payload);
                        try
                        {
                            var coins = await _dbProc.GetActiveTopCoinsAsync(); 
                            _logger.LogInformation("_dbProc.GetActiveTopCoinsAsync(); executed");
                    
                            _ymGenerator._ICoins = coins;
                            _ymGenerator.UpdateDataStackYaml();
                            _ymGenerator.UpdateDataFeedYaml();
                            
                            _tmuxService.SendCtrlCSignalToPane("datastack:1.0");
                            Thread.Sleep(2000);
                            _tmuxService.KillTmuxSession("datastack");
                            Thread.Sleep(2000);
                            _tmuxService.StartTmuxinatorSession(); 
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "GetActiveTopCoinsAsync() exception");
                        }
                    };

                    while (true)
                    {
                        await conn.WaitAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DB listener");
                }
            });
        }
    }
}