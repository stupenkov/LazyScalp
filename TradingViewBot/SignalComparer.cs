namespace TradingViewBot;

public class SignalComparer : ISignalComparer
{
    public SignalsType Compare(Signal oldSignal, Signal newSignal)
    {
        SignalsType signals = SignalsType.NoSignal;

        if (oldSignal.HighLevel == 0 && newSignal.HighLevel != 0)
            signals |= SignalsType.NewFormationHigh;

        if (oldSignal.LowLevel == 0 && newSignal.LowLevel != 0)
            signals |= SignalsType.NewFormationLow;

        if (oldSignal.HighLevel > 0 && newSignal.HighLevel > oldSignal.HighLevel)
            signals |= SignalsType.HighComing;

        if (oldSignal.LowLevel > 0 && newSignal.LowLevel > oldSignal.LowLevel)
            signals |= SignalsType.LowComing;

        if (oldSignal.HighLevel > 0 || newSignal.HighLevel > 0
            && (oldSignal.LowLevel == newSignal.LowLevel && oldSignal.HighLevel == newSignal.HighLevel))
            signals |= SignalsType.Repeat;

        return signals;
    }
}