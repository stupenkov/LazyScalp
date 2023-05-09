using Microsoft.Extensions.Options;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.Configurations;
using Stupesoft.LazyScalp.Domain.FinInstruments;
using Stupesoft.LazyScalp.Shared.Abstractions;
using Stupesoft.LazyScalp.Shared.Models;
using Stupesoft.LazyScalp.TradingView.Domain;

namespace Stupesoft.LazeScallp.Application.Servicies;

public class InstrumentDataExtractor : IInstrumentDataExtractor
{
    private IScreenshotAnalyzer _screenshotAnalyzer;
    private readonly IOptions<ApplicationOptions> _applicationOptions;

    public InstrumentDataExtractor(IScreenshotAnalyzer screenshotAnalyzer, IOptions<ApplicationOptions> applicationOptions)
    {
        _screenshotAnalyzer = screenshotAnalyzer;
        _applicationOptions = applicationOptions;
    }

    public async Task<FinInstrument.Data> ExtractAsync(FinInstrumentTV instrumentTradingView)
    {
        var signal = await _screenshotAnalyzer.AnalyzeAsync(instrumentTradingView.ChartImage!);
        var indicatorValues = _applicationOptions.Value.Features.UseColorDecoder
            ? await _screenshotAnalyzer.GetIndicatorValues(instrumentTradingView.ChartImage!)
            : new IndicatorValues();

        return new FinInstrument.Data
        {
            ChartImage = instrumentTradingView.ChartImage,
            Date = instrumentTradingView.LastUpdate,
            HLevelIndicator = new()
            {
                HighLevelDistance = signal.HighLevel,
                LowLevelDistance = signal.LowLevel,
            },
            HighLevelPrice = indicatorValues.HighLevel,
            LowLevelPrice = indicatorValues.LowLevel,
            BTCCorrelation = indicatorValues.Correlation,
            NATR = indicatorValues.NATR,
        };
    }
}
