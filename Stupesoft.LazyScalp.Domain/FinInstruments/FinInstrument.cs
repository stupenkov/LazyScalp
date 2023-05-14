namespace Stupesoft.LazyScalp.Domain.FinInstruments;

public class FinInstrument
{
    public int Id { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public List<Data> HistoryData { get; set; } = new();
    public DateTime? HighDetectionLevelTime { get; set; }
    public DateTime? LowDetectionLevelTime { get; set; }
    
    public class Data
    {
        public byte[]? ChartImage { get; set; }
        public HLevelIndicator HLevelIndicator { get; set; } = new HLevelIndicator();
        public DateTime Date { get; set; }
        public InstrumentState HighState { get; set; } = InstrumentState.None;
        public InstrumentState LowState { get; set; } = InstrumentState.None;
        public double HighLevelPrice { get; set; }
        public double LowLevelPrice { get; set; }
        public double NATR { get; set; }
        public double BTCCorrelation { get; set; }
    }
}
