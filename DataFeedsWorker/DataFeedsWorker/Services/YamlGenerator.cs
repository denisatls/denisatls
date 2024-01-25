using System.Text.RegularExpressions;
using DataFeedsWorker.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DataFeedsWorker.Services
{
    public class YamlFileConfig
    {
        public string DataStack { get; set; }
        public string DataFeed { get; set; }
    }
    
    public partial class YamlGenerator
    {
        private readonly YamlFileConfig _yamlFileConfig;
        private readonly ILogger<YamlGenerator> _logger;
        public IEnumerable<TopCoins> _ICoins { get; set; }

        public YamlGenerator(YamlFileConfig config, IEnumerable<TopCoins> ICoins, ILogger<YamlGenerator> logger)
        {
            _yamlFileConfig = config;
            _ICoins = ICoins;
            _logger = logger;
        }
        
        public void UpdateDataStackYaml()
        {
            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();

                var yamlContent = File.ReadAllText(_yamlFileConfig.DataStack);
                var dataStack = deserializer.Deserialize<DataStack>(yamlContent);

                UpdateYamlWithTopCoinsInDataStack(dataStack, _ICoins);

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                    .Build();

                var newYaml = serializer.Serialize(dataStack);
                File.WriteAllText(_yamlFileConfig.DataStack, newYaml);

                _logger.LogInformation("DataStack YAML updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating DataStack YAML");
            }
        }

        private void UpdateYamlWithTopCoinsInDataStack(DataStack dataStack, IEnumerable<TopCoins> topCoins)
        {
            try
            {
                var datafeedsWindow = dataStack.Windows.FirstOrDefault(window => window.Datafeeds != null);
                if (datafeedsWindow != null)
                {
                    var exchangeSymbols = topCoins
                        .Where(c => c.IsActive)
                        .GroupBy(c => c.ExchangeName)
                        .ToDictionary(
                            g => g.Key.ToLower(), 
                            g => g.Select(coin => coin.NormCoinId).ToList()); // Using NormCoinId directly


                    var updatedPanes = new List<string>();
                    foreach (var pane in datafeedsWindow.Datafeeds.Panes)
                    {
                        string updatedPane = pane;
                        foreach (var exchange in exchangeSymbols)
                        {
                            string newSymbols = string.Join(",", exchange.Value);
                            string pattern = $@"(\$DATAFEED_BIN {exchange.Key} trade -p )(\d+)( --symbols [^ ]+)";
                            updatedPane = Regex.Replace(updatedPane, pattern, m => $"{m.Groups[1].Value}{m.Groups[2].Value} --symbols {newSymbols}");
                        }
                        updatedPanes.Add(updatedPane);
                    }
                    datafeedsWindow.Datafeeds.Panes = updatedPanes;
                }
                _logger.LogInformation("Top coins in DataStack YAML updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating top coins in DataStack YAML");
            }
        }
        
        public void UpdateDataFeedYaml()
        {
            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();

                var yamlContent = File.ReadAllText(_yamlFileConfig.DataFeed);
                var feedsConfig = deserializer.Deserialize<FeedsConfig>(yamlContent);

                UpdateYamlWithTopCoinsInDataFeed(feedsConfig, _ICoins);

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();

                var newYaml = serializer.Serialize(feedsConfig);
                File.WriteAllText(_yamlFileConfig.DataFeed, newYaml);

                _logger.LogInformation("DataFeed YAML updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating DataFeed YAML");
            }
        }
        private void UpdateYamlWithTopCoinsInDataFeed(FeedsConfig feedsConfig, IEnumerable<TopCoins> topCoins)
        {
            try
            {
                var exchangeSymbols = topCoins
                    .Where(c => c.IsActive)
                    .GroupBy(c => c.CoingeckoId)
                    .ToDictionary(
                        g => g.Key, 
                        g => g.Select(coin => coin.NormCoinId).ToList()); // Direct use of NormCoinId


                foreach (var feed in feedsConfig.Feeds)
                {
                    // Find the matching exchange in the topCoins collection

                    if (exchangeSymbols.TryGetValue(feed.Exchange.ToLower(), out var symbols))
                    {
                        // Update only the Symbols property
                        feed.Symbols = symbols;
                    }
                }

                _logger.LogInformation("Symbols in DataFeed YAML updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating symbols in DataFeed YAML");
            }
        }

    }
}



