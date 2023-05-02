using Stupesoft.LazeScallp.Application.Abstractions;

namespace Stupesoft.LazeScallp.Application.Servicies;

public class LevelAnalyzer : ILevelAnalyzer
{
    public SignalType Analyze(int instrumentLevel, int signalLevel)
    {
        // Цена подошла к уровню
        if (instrumentLevel < 2 && signalLevel == 2)
        {
            return SignalType.PriceReachedLevel;
        }

        // Цена торгуется возле уровня 
        if (instrumentLevel > 0 && signalLevel > 0)
        {
            return SignalType.PriceIsNearLevel;
        }

        return SignalType.None;
    }
}

