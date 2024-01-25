namespace CoinManager.Models;

public class TopCoin
{
    public int Tradeseries_id { get; set; }
    public int Exchange_id { get; set; }
    public string Symbol { get; set; }
    public bool Active { get; set; }
}