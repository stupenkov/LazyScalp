namespace Stupesoft.LazyScalp.TradingView.Domain;

public interface IFinInstrumentTVManager
{
    FinInstrumentTV Create(byte[] image, string ticker);
}