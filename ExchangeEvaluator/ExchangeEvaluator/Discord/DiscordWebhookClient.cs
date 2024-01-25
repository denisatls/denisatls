using System.Text;
using Newtonsoft.Json;


namespace ExchangeEvaluator.Discord;

public class DiscordWebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;
    private readonly ILogger<DiscordWebhookClient> _logger;
    
    public DiscordWebhookClient(string webhookUrl,ILogger<DiscordWebhookClient> logger)
    {
        _webhookUrl = webhookUrl;
        _httpClient = new HttpClient();
        _logger = logger;
    }

    public async Task SendMessageAsync(string message)
    {
        var payload = new { content = message };
        var serializedPayload = JsonConvert.SerializeObject(payload);
        var requestContent = new StringContent(serializedPayload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_webhookUrl, requestContent);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to send message: {response.StatusCode} - {responseBody}");
        }
    }
}