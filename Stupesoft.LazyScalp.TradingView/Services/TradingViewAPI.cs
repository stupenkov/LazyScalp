using Stupesoft.LazyScalp.TradingView.Abstractions;
using Stupesoft.LazyScalp.TradingView.Domain;

namespace Stupesoft.LazyScalp.TradingView.Services;

internal class TradingViewAPI : ITradingViewAPI
{
    private readonly IFinInstrumentTVRepository _finInstrumentTradingViewRepository;
    private readonly IScanner _scaner;

    public TradingViewAPI(IFinInstrumentTVRepository finInstrumentTradingViewRepository, IScanner scaner)
    {
        _finInstrumentTradingViewRepository = finInstrumentTradingViewRepository;
        _scaner = scaner;
    }

    public IScanner Scaner => _scaner;

    public async Task<List<FinInstrumentTV>> GetAllInstrumentsAsync()
    {
        return (await _finInstrumentTradingViewRepository.GetAllAsync()).ToList();
    }
}
