namespace TradingViewBot;

[Flags]
public enum SignalsType
{
    NewFormationHigh = 10000,
    NewFormationLow = 01000,
    HighComing = 00100,
    LowComing = 00010,
    Repeat = 00001,
    NoSignal = 0,
}