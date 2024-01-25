using System.Data;
using Dapper;
using ExchangeEvaluator.Models;
using Npgsql;

namespace ExchangeEvaluator.Data;

public class Procedures
{
    private readonly IConfiguration _configuration;

    public Procedures(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    private IDbConnection CreateConnection()
    {
        string? connectionString = _configuration.GetConnectionString("ExchangeEvaluatorDB");
        return new NpgsqlConnection(connectionString);
    }

    public async Task<List<ExchangeList>> GetExchangeIdsAsync()
    {
        var sql = "SELECT exchange_id,coingecko_id FROM exchange;";
        using var connection = CreateConnection();
        var exchangeIds = await connection.QueryAsync<ExchangeList>(sql);
        return exchangeIds.ToList();
    }
    
    public async Task InsertDailyVolume(int exchangeId, DateTime date, decimal? volume)
    {
        var dateOnly = date.Date; // Get only the date part, without the time
        var sqlCheck = "SELECT COUNT(*) FROM daily_volume WHERE exchange_id = @ExchangeId AND date::date = @Date;";
        var sqlInsert = "INSERT INTO daily_volume (exchange_id, date, volume) VALUES (@ExchangeId, @Date, @Volume);";

        using var connection = CreateConnection();
        var exists = await connection.ExecuteScalarAsync<int>(sqlCheck, new { ExchangeId = exchangeId, Date = dateOnly });
        if (exists == 0)
        {
            await connection.ExecuteAsync(sqlInsert, new { ExchangeId = exchangeId, Date = date, Volume = volume });
        }
    }
    
    public async Task InsertTopCoin(int exchangeId, string coin,string normCoin)
    {
        var sqlInsert = "INSERT INTO trade_series (exchange_id, symbol,norm_symbol) VALUES (@ExchangeId, @Coin, @normCoin);";
        using var connection = CreateConnection();
        await connection.ExecuteAsync(sqlInsert, new { ExchangeId = exchangeId, Coin = coin,  NormCoin = normCoin });
    }

    public async Task InsertCoinHistory(int exchangeId, string coinId, DateTime date, decimal? volume)
    {
        var sql = "INSERT INTO coin_history (exchange_id, coin_id, date, volume) VALUES (@ExchangeId, @CoinId, @Date, @Volume); ";
        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, new { ExchangeId = exchangeId, CoinId = coinId, Date = date, Volume = volume });
    }
    
    public async Task<List<NewCoin>> GetNewCoinsAsync()
    {
        var sql = "SELECT * FROM get_new_coins();";
        using var connection = CreateConnection();
        return (await connection.QueryAsync<NewCoin>(sql)).ToList();
    }
    
    public async Task<List<OldCoin>> GetOldCoinsAsync()
    {
        var sql = "SELECT * FROM get_old_coins();";
        using var connection = CreateConnection();
        return (await connection.QueryAsync<OldCoin>(sql)).ToList();
    }

    
    public async Task InsertChangeAsync()
    {
            var query = "INSERT INTO changes (created_at, signal) VALUES (now(), true)";
            using var connection = CreateConnection();
            await connection.ExecuteAsync(query);
    }
    
    public async Task UpdateLastExecutionTime(string processName, DateTime lastExecution)
    {
        var sql = @"INSERT INTO execution_tracking (process_name, last_execution) 
                VALUES (@ProcessName, @LastExecution) 
                ON CONFLICT (process_name) 
                DO UPDATE SET last_execution = EXCLUDED.last_execution;";

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, new { ProcessName = processName, LastExecution = lastExecution });
    }

    public async Task<DateTime?> GetLastExecutionTime(string processName)
    {
        var sql = "SELECT last_execution FROM execution_tracking WHERE process_name = @ProcessName;";
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<DateTime?>(sql, new { ProcessName = processName });
    }
    
    public async Task<bool> CheckStartupTaskExecuted(string taskName)
    {
        var sql = "SELECT executed FROM startup_execution WHERE task_name = @TaskName;";
        using var connection = CreateConnection();
        var executed = await connection.QueryFirstOrDefaultAsync<bool>(sql, new { TaskName = taskName });
        return executed;
    }
    
    public async Task MarkStartupTaskAsExecuted(string taskName)
    {
        var sql = @"
        INSERT INTO startup_execution (task_name, executed) 
        VALUES (@TaskName, TRUE) 
        ON CONFLICT (task_name) 
        DO UPDATE SET executed = EXCLUDED.executed;";

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, new { TaskName = taskName });
    }
}

