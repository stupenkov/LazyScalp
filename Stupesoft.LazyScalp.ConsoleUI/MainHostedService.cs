using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazyScalp.Domain.FinInstruments;
using Stupesoft.LazyScalp.Shared.Abstractions;
using Stupesoft.LazyScalp.TradingView.Abstractions;

namespace Stupesoft.LazyScalp.ConsoleUI;

public class MainHostedService : BackgroundService
{
    private readonly ITradingViewAPI _tradingViewAPI;
    private readonly IFinInstrumentRepository _finInstrumentRepository;
    private readonly IInstrumentDataExtractor _instrumentDataExtractor;
    private readonly IFinInstrumentManager _finInstrumentManager;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IInstrumentAnslyzer _instrumentAnslyzer;
    private readonly ISender _sender;
    private readonly ILogger<MainHostedService> _logger;

    public MainHostedService(
        IDateTimeProvider dateTimeProvider,
        IInstrumentAnslyzer instrumentAnslyzer,
        IInstrumentDataExtractor instrumentDataExtractor,
        ISender sender,
        ITradingViewAPI tradingViewAPI,
        IFinInstrumentRepository finInstrumentRepository,
        IFinInstrumentManager finInstrumentManager,
        ILogger<MainHostedService> logger)
    {
        _tradingViewAPI = tradingViewAPI;
        _finInstrumentRepository = finInstrumentRepository;
        _finInstrumentManager = finInstrumentManager;
        _dateTimeProvider = dateTimeProvider;
        _instrumentAnslyzer = instrumentAnslyzer;
        _instrumentDataExtractor = instrumentDataExtractor;
        _sender = sender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _tradingViewAPI.Scaner.InstrumentReady += async (instrumentTV) =>
        {
            DateTime currDate = _dateTimeProvider.GetCurrentTime();
            FinInstrument.Data instrumentData = await _instrumentDataExtractor.ExtractAsync(instrumentTV);
            FinInstrument instrument = (await _finInstrumentRepository.FindByTickerAsync(instrumentTV.Ticker!));
            instrument ??= _finInstrumentManager.Create(instrumentTV.Ticker!);
            _finInstrumentManager.AddData(instrument, instrumentData);
            await _finInstrumentRepository.AddOrUpdateAsync(instrument);
            var signals = await _instrumentAnslyzer.AnalyzeAsync(instrument);
            await _sender.SendAsync(signals, instrument);
        };

        _logger.LogInformation("Runing instrument scanner...");
        await _tradingViewAPI.Scaner.RunAsync(stoppingToken);
    }
}
