namespace Stupesoft.LazyScalp.Domain.FinInstruments;

public class FinInstrumentManager : IFinInstrumentManager
{
    public FinInstrument Create(string Ticker)
    {
        if (string.IsNullOrEmpty(Ticker))
            throw new ArgumentException($"'{nameof(Ticker)}' cannot be null or empty.", nameof(Ticker));

        return new FinInstrument
        {
            Id = 0,
            Ticker = Ticker,
            HistoryData = new List<FinInstrument.Data>
            {
                new(),
                new(),
            }
        };
    }

    public void AddData(FinInstrument finInstrument, FinInstrument.Data data)
    {
        finInstrument.HistoryData.Add(data);

        if (finInstrument.HistoryData.Count > 2)
        {
            finInstrument.HistoryData.RemoveAt(0);
        }

        var prevData = finInstrument.HistoryData[^2];
        var currData = finInstrument.HistoryData[^1];

        InstrumentState high = InstrumentState.None;
        InstrumentState low = InstrumentState.None;

        if (prevData.HighState == InstrumentState.None && currData.HLevelIndicator.HighLevelDistance == 2)
            high = InstrumentState.ReachedLevel;
        else if ((prevData.HighState == InstrumentState.ReachedLevel || prevData.HighState == InstrumentState.TradingNearLevel)
            && currData.HLevelIndicator.HighLevelDistance > 0)
            high = InstrumentState.TradingNearLevel;

        if (prevData.LowState == InstrumentState.None && currData.HLevelIndicator.LowLevelDistance == 2)
            low = InstrumentState.ReachedLevel;
        else if ((prevData.LowState == InstrumentState.ReachedLevel || prevData.LowState == InstrumentState.TradingNearLevel)
            && currData.HLevelIndicator.LowLevelDistance > 0)
            low = InstrumentState.TradingNearLevel;

        currData.HighState = high;
        if (currData.HighState == InstrumentState.None)
            finInstrument.HighDetectionLevelTime = null;
        else if (currData.HighState == InstrumentState.ReachedLevel)
            finInstrument.HighDetectionLevelTime = data.Date;

        currData.LowState = low;
        if (currData.LowState == InstrumentState.None)
            finInstrument.LowDetectionLevelTime = null;
        else if (currData.LowState == InstrumentState.ReachedLevel)
            finInstrument.LowDetectionLevelTime = data.Date;
    }
}
