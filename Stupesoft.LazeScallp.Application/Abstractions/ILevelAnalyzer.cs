using Stupesoft.LazeScallp.Application.Servicies;

namespace Stupesoft.LazeScallp.Application.Abstractions;

public interface ILevelAnalyzer
{
    ResultOfLevel Analyze(int instrumentHighLevel, DateTime? detectionTime, int signalHighLevel);
}