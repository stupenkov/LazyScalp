namespace Stupesoft.LazyScalp.Domain.FinInstruments;

public interface IFinInstrumentManager
{
    void AddData(FinInstrument finInstrument, FinInstrument.Data data);
    FinInstrument Create(string Ticker);
}