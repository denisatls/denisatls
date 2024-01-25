using Dapper;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataFeedsWorker.Models;
using Microsoft.Extensions.Configuration;

namespace DataFeedsWorker.Data
{
    public class Procedures
    {
        private readonly string _connectionString;

        public Procedures(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ExchangeEvaluatorDB");
        }

        public async Task<IEnumerable<TopCoins>> GetActiveTopCoinsAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            const string query = @"
       SELECT
            tc.tradeseries_id as TopCoinId,
            e.exchange_id AS ExchangeId,
            e.name AS ExchangeName,
            e.coingecko_id as CoingeckoId,
            tc.symbol AS Coin,
            tc.active AS IsActive,
            tc.norm_symbol as NormCoinId
        FROM
            trade_series AS tc
        INNER JOIN
            exchange AS e ON tc.exchange_id = e.exchange_id
        ORDER BY
            tc.tradeseries_id;";

            var result = await connection.QueryAsync<TopCoins>(query);
            return result;
        }
    }
}