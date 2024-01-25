using YamlDotNet.Serialization;

namespace DataFeedsWorker.Models;

public class DataStack
{
    public string Name { get; set; }
    public string Root { get; set; }
    public string PreWindow { get; set; }

    [YamlMember(Alias = "windows")]
    public List<Window> Windows { get; set; }
}

public class Window
{
    public Datafeeds Datafeeds { get; set; }
    public Recorder Recorder { get; set; }
}

public class Datafeeds
{
    public string Layout { get; set; }

    [YamlMember(Alias = "panes")]
    public List<string> Panes { get; set; }
}

public class Recorder
{
    public string Layout { get; set; }

    [YamlMember(Alias = "panes")]
    public List<string> Panes { get; set; }
}