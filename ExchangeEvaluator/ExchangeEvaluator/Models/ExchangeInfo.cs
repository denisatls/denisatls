using System;

namespace ExchangeEvaluator.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;


    public class ExchangeInfo
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("year_established")] public int? YearEstablished { get; set; }

        [JsonProperty("country")] public string Country { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("url")] public string Url { get; set; }

        [JsonProperty("image")] public string Image { get; set; }

        [JsonProperty("facebook_url")] public string FacebookUrl { get; set; }

        [JsonProperty("reddit_url")] public string RedditUrl { get; set; }

        [JsonProperty("telegram_url")] public string TelegramUrl { get; set; }

        [JsonProperty("slack_url")] public string SlackUrl { get; set; }

        [JsonProperty("other_url_1")] public string OtherUrl1 { get; set; }

        [JsonProperty("other_url_2")] public string OtherUrl2 { get; set; }

        [JsonProperty("twitter_handle")] public string TwitterHandle { get; set; }

        [JsonProperty("has_trading_incentive")]
        public bool? HasTradingIncentive { get; set; }

        [JsonProperty("centralized")] public bool Centralized { get; set; }

        [JsonProperty("public_notice")] public string PublicNotice { get; set; }

        [JsonProperty("alert_notice")] public string AlertNotice { get; set; }

        [JsonProperty("trust_score")] public int TrustScore { get; set; }

        [JsonProperty("trust_score_rank")] public int TrustScoreRank { get; set; }

        [JsonProperty("trade_volume_24h_btc")] public decimal? TradeVolume24hBtc { get; set; }

        [JsonProperty("trade_volume_24h_btc_normalized")]
        public decimal? TradeVolume24hBtcNormalized { get; set; }

        [JsonProperty("tickers")] public List<Ticker> Tickers { get; set; }

        [JsonProperty("status_updates")] public List<StatusUpdate> StatusUpdates { get; set; }
    }

    public class Ticker
    {
        [JsonProperty("base")] public string Base { get; set; }

        [JsonProperty("target")] public string Target { get; set; }

        [JsonProperty("market")] public Market Market { get; set; }

        [JsonProperty("last")] public decimal? Last { get; set; }

        [JsonProperty("volume")] public decimal? Volume { get; set; }

        [JsonProperty("converted_last")] public ConvertedLast ConvertedLast { get; set; }

        [JsonProperty("converted_volume")] public ConvertedVolume ConvertedVolume { get; set; }

        [JsonProperty("trust_score")] public string TrustScore { get; set; }

        [JsonProperty("bid_ask_spread_percentage")]
        public decimal? BidAskSpreadPercentage { get; set; }

        [JsonProperty("timestamp")] public DateTime Timestamp { get; set; }

        [JsonProperty("last_traded_at")] public DateTime LastTradedAt { get; set; }

        [JsonProperty("last_fetch_at")] public DateTime LastFetchAt { get; set; }

        [JsonProperty("is_anomaly")] public bool IsAnomaly { get; set; }

        [JsonProperty("is_stale")] public bool IsStale { get; set; }

        [JsonProperty("trade_url")] public string TradeUrl { get; set; }

        [JsonProperty("token_info_url")] public string TokenInfoUrl { get; set; }

        [JsonProperty("coin_id")] public string CoinId { get; set; }

        [JsonProperty("target_coin_id")] public string TargetCoinId { get; set; }
    }

    public class Market
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("identifier")] public string Identifier { get; set; }

        [JsonProperty("has_trading_incentive")]
        public bool HasTradingIncentive { get; set; }
    }

    public class ConvertedLast
    {
        [JsonProperty("btc")] public decimal? Btc { get; set; }

        [JsonProperty("eth")] public decimal? Eth { get; set; }

        [JsonProperty("usd")] public decimal? Usd { get; set; }
    }

    public class ConvertedVolume
    {
        [JsonProperty("btc")] public decimal? Btc { get; set; }

        [JsonProperty("eth")] public decimal? Eth { get; set; }

        [JsonProperty("usd")] public decimal? Usd { get; set; }
    }

    public class StatusUpdate
    {
        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("category")] public string Category { get; set; }

        [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }

        [JsonProperty("user")] public string User { get; set; }

        [JsonProperty("user_title")] public string UserTitle { get; set; }

        [JsonProperty("pin")] public bool Pin { get; set; }

        [JsonProperty("project")] public Project Project { get; set; }
    }

    public class Project
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("image")] public ProjectImage Image { get; set; }
    }

    public class ProjectImage
    {
        [JsonProperty("thumb")] public string Thumb { get; set; }

        [JsonProperty("small")] public string Small { get; set; }

        [JsonProperty("large")] public string Large { get; set; }
    }



}


