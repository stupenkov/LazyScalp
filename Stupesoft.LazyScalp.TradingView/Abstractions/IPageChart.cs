using Stupesoft.LazyScalp.TradingView.Domain;

namespace Stupesoft.LazyScalp.TradingView.Abstractions;

public interface IPageChart
{
    void ReloadBrowser();
    Task LoginAsync(string username, string password);
    Task SetChartTemplateAsync();
    Task<bool> IsOpenScreenerAsync();
    Task OpenScreenerAsync();
    Task CloseScreenerAsync();
    Task UpdateScreenerDataAsync();
    Task<int> CountScreenerInstrumentsAsync();
    Task SelectInstrumentAsync(int index);
    Task<FinInstrumentTV> GetInstrumentAsync(int index);
    Task<bool> IsOpenAdsToastAsync();
    Task CloseAdsToastAsync();
    Task InputTickerAsync(string name);
    Task RefreshPageAsync();

    // TODO: Task ChangeTimeFrame(TimeFrame timeFrame);
}
