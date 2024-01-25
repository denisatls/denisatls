namespace ExchangeEvaluator.Models;

public record NewCoin
{
    public DateTime Bucket { get; set; }
    public int Exchange_id { get; set; }
    public string Exchange_name { get; set; }
    public string Coin_id { get; set; }
    public string Norm_Coin_Id { get; set; } 
}


public record OldCoin
{
    public DateTime Bucket { get; set; }
    public int Exchange_id { get; set; }
    public string Exchange_name { get; set; }
    public string Coin_id { get; set; }
    public string Norm_Coin_Id { get; set; } 
}