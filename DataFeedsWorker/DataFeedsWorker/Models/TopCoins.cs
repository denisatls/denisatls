
namespace DataFeedsWorker.Models;

public record TopCoins
{
    public int TopCoinId { get; set; }
    public int ExchangeId { get; set; }
    public string ExchangeName { get; set; }
    public string Coin { get; set; }
    public bool IsActive { get; set; }
    public string  CoingeckoId { get; set; }
    
    public string NormCoinId { get; set; }
}