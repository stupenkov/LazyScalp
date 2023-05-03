namespace Stupesoft.LazyScalp.TradingView.Domain;

public class FinInstrumentTV
{
    public int Id { get; set; }
    public string? Ticker { get; set; }
    public byte[]? ChartImage { get; set; }
    public DateTime LastUpdate { get; set; }
}
