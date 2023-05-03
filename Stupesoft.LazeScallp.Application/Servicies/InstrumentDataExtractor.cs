using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazyScalp.Domain.FinInstruments;
using Stupesoft.LazyScalp.Shared.Abstractions;
using Stupesoft.LazyScalp.TradingView.Domain;

namespace Stupesoft.LazeScallp.Application.Servicies;

public class InstrumentDataExtractor : IInstrumentDataExtractor
{
    private IScreenshotAnalyzer _screenshotAnalyzer;

    public InstrumentDataExtractor(IScreenshotAnalyzer screenshotAnalyzer)
    {
        _screenshotAnalyzer = screenshotAnalyzer;
    }

    public async Task<FinInstrument.Data> ExtractAsync(FinInstrumentTV instrumentTradingView)
    {
        var signal = await _screenshotAnalyzer.AnalyzeAsync(instrumentTradingView.ChartImage!);
        return new FinInstrument.Data
        {
            ChartImage = instrumentTradingView.ChartImage,
            Date = instrumentTradingView.LastUpdate,
            HLevelIndicator = new()
            {
                HighLevelDistance = signal.HighLevel,
                LowLevelDistance = signal.LowLevel,
            },
        };
    }
}
