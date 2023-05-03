namespace Stupesoft.LazyScalp.TradingView.Domain;
internal interface IFinInstrumentTVRepository
{
    Task AddOrUpdateAsync(FinInstrumentTV entity);
    Task<FinInstrumentTV> FindByTickerAsync(string name);
    Task<IEnumerable<FinInstrumentTV>> GetAllAsync();
}
