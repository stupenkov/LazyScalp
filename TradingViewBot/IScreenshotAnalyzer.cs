namespace TradingViewBot;

public interface IScreenshotAnalyzer
{
    Task<bool> HasSignalAsync(byte[] image);
    Task<bool> IndicatorLoadedAsync(byte[] image);
    Task<Signal> AnalyzeAsync(byte[] image);
}
