using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazyScalp.Domain.Instrument;
using Stupesoft.LazyScalp.Domain.Notification;

namespace Stupesoft.LazeScallp.Application.Servicies;

public class SignalAnalyzer : ISignalAnalyser
{
    private readonly TimeSpan _lagTime = TimeSpan.FromMinutes(60);
    private readonly ILevelAnalyzer _levelAnalyzer;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SignalAnalyzer(ILevelAnalyzer levelAnalyzer, IDateTimeProvider dateTimeProvider)
    {
        _levelAnalyzer = levelAnalyzer;
        _dateTimeProvider = dateTimeProvider;
    }

    public ResultOfSignals Analyze(Instrument instrument, Signal signal, Notification notification)
    {
        DateTime currentTime = _dateTimeProvider.GetCurrentTime();

        SignalType signalTypeHigh = _levelAnalyzer.Analyze(instrument.HighLevel, signal.HighLevel);
        bool itsTimeHigh = currentTime - notification.LastHigh > _lagTime;
        ResultOfLevel resultHigh;
        if (signalTypeHigh == SignalType.PriceReachedLevel)
        {
            resultHigh = new ResultOfLevel(SignalType.PriceReachedLevel, TimeSpan.Zero);
        }
        else if (signalTypeHigh == SignalType.PriceIsNearLevel && itsTimeHigh)
        {
            resultHigh = new ResultOfLevel(SignalType.PriceIsNearLevel, currentTime - instrument.HighDetectionTime!.Value);
        }
        else
        {
            resultHigh = new ResultOfLevel(SignalType.None, TimeSpan.Zero);
        }

        SignalType signalTypeLow = _levelAnalyzer.Analyze(instrument.LowLevel, signal.LowLevel);
        bool itsTimeLow = currentTime - notification.LastLow > _lagTime;
        ResultOfLevel resultLow;
        if (signalTypeLow == SignalType.PriceReachedLevel)
        {
            resultLow = new ResultOfLevel(SignalType.PriceReachedLevel, TimeSpan.Zero);
        }
        else if (signalTypeLow == SignalType.PriceIsNearLevel && itsTimeLow)
        {
            resultLow = new ResultOfLevel(SignalType.PriceIsNearLevel, currentTime - instrument.LowDetectionTime!.Value);
        }
        else
        {
            resultLow = new ResultOfLevel(SignalType.None, TimeSpan.Zero);
        }

        return new ResultOfSignals(resultHigh, resultLow);
    }
}

