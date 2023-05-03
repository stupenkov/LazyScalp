﻿namespace Stupesoft.LazyScalp.Domain.FinInstruments;

public class FinInstrument
{
    public int Id { get; set; }
    public string? Ticker { get; set; }
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
    }
}
