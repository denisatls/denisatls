using ExchangeEvaluator.Data;
namespace ExchangeEvaluator.Services;

public class ExchangeProcessingService
{
    private readonly ExchangeService _exchangeService;
    private readonly ILogger<ExchangeProcessingService> _logger;
    private readonly Procedures _databaseService;
    private readonly CoinEvaluator _coinEvaluator;
    private bool isInsertTopCoinExecuted { get; set; } = false;

    public ExchangeProcessingService(ExchangeService exchangeService, Procedures databaseService, ILogger<ExchangeProcessingService> logger, CoinEvaluator coinEvaluator)
    {
        _exchangeService = exchangeService;
        _databaseService = databaseService;
        _logger = logger;
        _coinEvaluator = coinEvaluator;
    }
    
    public async Task ProcessExchangesAsync()
    {
        _logger.LogInformation("Starting ProcessExchangesAsync");
        try
        {
            var lastNewCoinEvaluation = await _databaseService.GetLastExecutionTime("NewCoinEvaluation") ?? DateTime.MinValue;// this change is not on prod publish  it !!!!!! (DateTime.Today was)
            var lastOldCoinEvaluation = await _databaseService.GetLastExecutionTime("OldCoinEvaluation") ?? DateTime.MinValue;
        
            
            if ((DateTime.Now - lastNewCoinEvaluation).TotalDays >= 3)
            {
                _logger.LogInformation("Evaluating new coins");
                await _coinEvaluator.EvaluateNewCoinsAsync();
                lastNewCoinEvaluation = DateTime.Now;
                await _databaseService.UpdateLastExecutionTime("NewCoinEvaluation", lastNewCoinEvaluation);
                _logger.LogInformation("Evaluating new coins done");
            }
            
            if ((DateTime.Now - lastOldCoinEvaluation).TotalDays >= 10)
            {
                _logger.LogInformation("Evaluating old coins");
                await _coinEvaluator.EvaluateOldCoinsAsync();
                lastOldCoinEvaluation = DateTime.Now;
                await _databaseService.UpdateLastExecutionTime("OldCoinEvaluation", lastOldCoinEvaluation);
                _logger.LogInformation("Evaluating old coins done");
            }

            var allExchangesFromCoinG = await _exchangeService.GetAllExchangesAsync();
            var exchangeIdsFromDb = await _databaseService.GetExchangeIdsAsync();
            
            _exchangeService.CheckExchangesExistence(exchangeIdsFromDb, allExchangesFromCoinG);

            foreach (var exchangeId in exchangeIdsFromDb)
            {
                await ProcessExchangeAsync(exchangeId.coingecko_id, exchangeId.exchange_id);
                await UpdateDayVolume(exchangeId.coingecko_id, exchangeId.exchange_id);
            }
            
            await _databaseService.MarkStartupTaskAsExecuted("InsertTopCoin");
            
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error encountered in ProcessExchangesAsync");
        }
        _logger.LogInformation("Completed ProcessExchangesAsync");
    }

    private async Task UpdateDayVolume(string CoinGexchangeId, int exchangeId)
    {
        _logger.LogInformation($"Updating daily volume for Exchange ID: {exchangeId}");
        var today = DateTime.Now.Date;
        var dv = await _exchangeService.GetCoinAsync(CoinGexchangeId);
        await _databaseService.InsertDailyVolume(exchangeId, today, dv?.TradeVolume24hBtcNormalized);
    }
    private async Task ProcessExchangeAsync(string coinGexchangeId, int exchangeId)
    {
        _logger.LogInformation($"Processing exchange ID: {coinGexchangeId}  await Task.Delay(30000)" );
        await Task.Delay(30000); // Simulating a delay (must to do Coingecko will bloc us )
        
        var topCoins = await _exchangeService.GetTopTradedPairsAsync(coinGexchangeId);
        
        isInsertTopCoinExecuted = await _databaseService.CheckStartupTaskExecuted("InsertTopCoin");
        
        if (topCoins != null && topCoins.Count > 0)
        {
            foreach (var coin in topCoins)
            {
                string marketCoin = string.Empty;
                string normCoin = string.Empty;

                if (exchangeId == 7)
                {
                    marketCoin = $"{coin.Target}-{coin.Base}";
                    normCoin = $"SPOT.{coin.Target}.{coin.Base}";
                }
                else if (exchangeId == 4) //Paribu
                {
                    marketCoin = $"{coin.Base}-TL";
                    normCoin = $"SPOT.{coin.Base}.TL";
                }
                else
                {
                    marketCoin = $"{coin.Base}-{coin.Target}";
                    normCoin = $"SPOT.{coin.Base}.{coin.Target}";
                }

                if (!isInsertTopCoinExecuted)
                {
                    await _databaseService.InsertTopCoin(exchangeId, marketCoin, normCoin);
                }
                
                await _databaseService.InsertCoinHistory(exchangeId, marketCoin, DateTime.Now, coin.ConvertedVolume.Btc);
                _logger.LogInformation($"Processed coin history for {coin.Base}-{coin.Target}");
            }
        }
        else
        {
            _logger.LogWarning($"No top coins found or there was an error for exchange ID: {exchangeId}");
        }
        _logger.LogInformation($"Finished processing exchange ID: {coinGexchangeId}");
    }
}
