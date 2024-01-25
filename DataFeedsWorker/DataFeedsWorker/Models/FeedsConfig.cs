namespace DataFeedsWorker.Models;


    public class FeedsConfig
    {
        public List<Feed> Feeds { get; set; }
    }

    public class Feed
    {
        public string Address { get; set; }
        public string Exchange { get; set; }
        public string StreamName { get; set; }
        public List<string> EventTypes { get; set; }
        public List<string> Symbols { get; set; }
    }
