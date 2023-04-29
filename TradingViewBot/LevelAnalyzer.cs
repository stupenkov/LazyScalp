namespace TradingViewBot;

public class LevelAnalyzer
{
    private IDateTimeProvider _dateTimeProvider;

    public LevelAnalyzer(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public ResultOfLevel Analyze(int instrumentHighLevel, DateTime? detectionTime, int signalHighLevel)
    {
        SignalType signalType;
        TimeSpan time = TimeSpan.Zero;

        if (IsPriceFar(signalHighLevel))
            signalType = SignalType.PriceIsFarFromLevel;
        else if (IsPriceApproachedLevel(instrumentHighLevel, signalHighLevel))
            signalType = SignalType.PriceApproachedLevel;
        else if (IsPriceReachedLevel(instrumentHighLevel, signalHighLevel))
            signalType = SignalType.PriceReachedLevel;
        else if (IsPriceNearLevel(instrumentHighLevel, signalHighLevel))
            signalType = SignalType.PriceIsNearLevel;
        else
            throw new NotImplementedException();

        if (detectionTime.HasValue)
            time = GetTimeNearLevel(detectionTime, _dateTimeProvider.GetCurrentTime());

        return new ResultOfLevel(signalType, time);
    }

    private bool IsPriceReachedLevel(int oldLevel, int newLevel) => oldLevel == 0 && newLevel > 0;

    private bool IsPriceApproachedLevel(int oldLevel, int newLevel) => oldLevel == 1 && newLevel > 1;

    private bool IsPriceNearLevel(int oldLevel, int newLevel) => oldLevel > 0 && newLevel > 0;

    private bool IsPriceFar(int newLevel) => newLevel == 0;

    private TimeSpan GetTimeNearLevel(DateTime? detectionTime, DateTime currentTime)
    {
        if (detectionTime.HasValue)
            return currentTime - detectionTime.Value;

        return TimeSpan.Zero;
    }

}

