namespace TradingViewBot;

public interface ISignalComparer
{
    SignalsType Compare(Signal oldSignal, Signal newSignal);
}
