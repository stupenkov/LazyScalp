using Stupesoft.LazyScalp.Shared.Models;

namespace Stupesoft.LazyScalp.Shared.Abstractions;

public interface IScreenshotAnalyzer
{
    Task<bool> HasSignalAsync(byte[] image);
    Task<bool> IndicatorLoadedAsync(byte[] image);
    Task<HLevelSignal> AnalyzeAsync(byte[] image);
    Task<IndicatorValues> GetIndicatorValues(byte[] image);
}
