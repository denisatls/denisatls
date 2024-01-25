using System.Data;
using CoinManager.Models;
using Dapper;


namespace CoinManager.Data;

public class Procedures
{
    private readonly IDbConnection _dbConnection;

    public Procedures(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IEnumerable<Exchange>> GetAllExchangesAsync()
    {
        var query = "SELECT * FROM exchange";
        return await _dbConnection.QueryAsync<Exchange>(query);
    }

    public async Task<IEnumerable<TopCoin>> GetTopCoinsByExchangeAsync(int exchangeId)
    {
        var query = "SELECT * FROM trade_series WHERE exchange_id = @ExchangeId";
        return await _dbConnection.QueryAsync<TopCoin>(query, new { ExchangeId = exchangeId });
    }

    public async Task<IEnumerable<TopCoin>> GetAllTopCoinsAsync()
    {
        var query = "SELECT * FROM trade_series";
        return await _dbConnection.QueryAsync<TopCoin>(query);
    }

    public async Task<bool> UpdateCoinActiveStatusAsync(int exchangeId, string coinName, bool isActive)
    {
        
            var query = "UPDATE trade_series SET active = @IsActive WHERE exchange_id = @ExchangeId AND symbol = @CoinName";
            var result = await _dbConnection.ExecuteAsync(query, new { IsActive = isActive, ExchangeId = exchangeId, CoinName = coinName });
            return result > 0; 
    }
    
    public async Task InsertChangeAsync()
    {
            var query = "INSERT INTO changes (created_at, signal) VALUES (now(), true)";
            await _dbConnection.ExecuteAsync(query);
    }
    
    public async Task<IEnumerable<TopCoin>> GetTopCoinsByExchangeAndStatusAsync(int exchangeId, bool isActive)
    {
        var query = "SELECT * FROM trade_series WHERE exchange_id = @ExchangeId AND active = @IsActive";
        
        var result  = await _dbConnection.QueryAsync<TopCoin>(query, new { ExchangeId = exchangeId, IsActive = isActive });
        return result;
    }

}
