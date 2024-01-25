using System.Text;
using ExchangeEvaluator.Data;
using ExchangeEvaluator.Discord;
using ExchangeEvaluator.Models;

namespace ExchangeEvaluator.Services;

public class CoinEvaluator
{
    private readonly ILogger<CoinEvaluator> _logger;
    private readonly Procedures _databaseService;
    private readonly DiscordWebhookClient _discordWebhookClient;
    
    public CoinEvaluator(ILogger<CoinEvaluator> logger, Procedures databaseService, DiscordWebhookClient discordWebhookClient) 
    {
        _logger = logger;
        _databaseService = databaseService;
        _discordWebhookClient = discordWebhookClient; 
    }
    
    
    public async Task EvaluateOldCoinsAsync()
    {
        try
        {
            var oldCoins = await _databaseService.GetOldCoinsAsync();
            if (oldCoins.Count > 0)
            {
                StringBuilder messageBuilder = new StringBuilder("Old coins detected (not in top list for 10 days):\n");
                foreach (var coin in oldCoins)
                {
                    string coinMessage = $"- {coin.Coin_id} on exchange {coin.Exchange_name} has not been in the top list for 10 days.\n";
                
                    if (messageBuilder.Length + coinMessage.Length > 2000)
                    {
                        await SendMessageAndLog(messageBuilder.ToString());
                        messageBuilder.Clear();
                        messageBuilder.Append("Continued:\n");
                    }
                    
                    messageBuilder.Append(coinMessage);
                    _logger.LogInformation($"Processed old coin {coin.Coin_id} on exchange {coin.Exchange_name}");
                }
                if (messageBuilder.Length > 0)
                {
                    await SendMessageAndLog(messageBuilder.ToString());
                }
            }
            else
            {
                _logger.LogInformation("No old coins detected at this time");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in EvaluateOldCoinsAsync()");
        }
    }

    public async Task EvaluateNewCoinsAsync()
    {
        try
        {
            var newCoins = await _databaseService.GetNewCoinsAsync();
            if (newCoins.Count > 0)
            {
                StringBuilder messageBuilder = new StringBuilder("New coins detected:\n");
                foreach (var coin in newCoins)
                {
                    string coinMessage = $"- {coin.Coin_id} on exchange {coin.Exchange_name}\n";
                    
                    if (messageBuilder.Length + coinMessage.Length > 2000)
                    {
                        await SendMessageAndLog(messageBuilder.ToString());
                        messageBuilder.Clear(); 
                        messageBuilder.Append("Continued:\n"); 
                    }

                    messageBuilder.Append(coinMessage); 
                    
                    await  _databaseService.InsertTopCoin(coin.Exchange_id, coin.Coin_id, ConvertCoinPairToNormFormat(coin.Coin_id));
                    _logger.LogInformation("_databaseService.InsertTopCoin(coin.Exchange_id, coin.Coin_id); executed {Coin}",coin.Coin_id);
                }
                if (messageBuilder.Length > 0)
                {
                    await SendMessageAndLog(messageBuilder.ToString());
                }
                
                 //This will trigger DbListener in DataFeedWorker and recreate datastack.yaml
                await _databaseService.InsertChangeAsync();
                _logger.LogInformation("await _databaseService.InsertChangeAsync(); executed");
            }
            else
            {
                _logger.LogInformation("No new coins detected at this time");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in EvaluateCoinsAsync()");
        }
    }

    private async Task SendMessageAndLog(string message)
    {
        _logger.LogInformation($"Sending message to Discord: {message}");
        await _discordWebhookClient.SendMessageAsync(message);
    }
    
    
    private string ConvertCoinPairToNormFormat(string coinPair)
    {
        var parts = coinPair.Split('-');
        if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid coin pair format", nameof(coinPair));
        }
        return $"SPOT.{parts[0]}.{parts[1]}";
    }
}