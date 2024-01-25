
using ExchangeEvaluator.Models;
using Newtonsoft.Json;

namespace ExchangeEvaluator.Services;

public class ExchangeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _baseApiUrl = "https://api.coingecko.com/api/v3/exchanges";
    private readonly ILogger<ExchangeService> _logger;

    public ExchangeService(IHttpClientFactory httpClientFactory, ILogger<ExchangeService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ExchangeInfo?> GetCoinAsync(string exchangeId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            string requestUrl = $"{_baseApiUrl}/{exchangeId}";
            var response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ExchangeInfo>(responseContent);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"Error fetching market data: {e.Message}");
            return null;
        }
    }
    
    public async Task<List<ExchangeResponse>?> GetAllExchangesAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(_baseApiUrl);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ExchangeResponse>>(responseContent);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"Error fetching all exchanges: {e.Message}");
            return null;
        }
    }
    
    public void CheckExchangesExistence(List<ExchangeList> predefinedExchanges, List<ExchangeResponse> allExchanges)
    {
        var allExchangeIds = allExchanges.Select(e => e.Id).ToHashSet();

        foreach (var exchange in predefinedExchanges)
        {
            if (!allExchangeIds.Contains(exchange.coingecko_id))
            {
                throw new InvalidOperationException($"Exchange with ID '{exchange}' not found in the available exchanges.");
            }
            else
            {
                _logger.LogInformation($"Exchange with ID '{exchange}' found");
            }
        }
    }

 
    public async Task<List<Ticker>?> GetTopTradedPairsAsync(string exchangeId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            string requestUrl = $"{_baseApiUrl}/{exchangeId}/tickers";
            var response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            // Read the response as a string
            string jsonResponse = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON string to ExchangeInfo object
            var exchangeInfo = JsonConvert.DeserializeObject<ExchangeInfo>(jsonResponse);

            if (exchangeInfo != null && exchangeInfo.Tickers != null)
            {
                return exchangeInfo.Tickers
                    .OrderByDescending(t => t.ConvertedVolume.Usd)
                    .Take(10)
                    .ToList();
            }
            return null;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"HTTP request error fetching top traded pairs: {e.Message}");
            return null;
        }
        catch (JsonException e)
        {
            _logger.LogError($"JSON deserialization error fetching top traded pairs: {e.Message}");
            return null;
        }
    }

}