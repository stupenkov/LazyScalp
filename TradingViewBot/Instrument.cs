namespace TradingViewBot;

public class Instrument
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int HighLevel { get; set; }
    public int LowLevel { get; set; }
    public DateTime LastUpdate { get; set; }
}

