using Stupesoft.LazeScallp.Application.Servicies;

namespace Stupesoft.LazeScallp.Application.Abstractions;

public interface ILevelAnalyzer
{
    SignalType Analyze(int instrumentLevel, int signalLevel);
}