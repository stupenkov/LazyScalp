namespace SmartScalp.TradingView;

public interface ITradingView
{
    Task LoginAsync(string username, string password);
    Task SetChartTemplateAsync();
    Task<bool> IsOpenScreenerAsync();
    Task OpenScreenerAsync();
    Task CloseScreenerAsync();
    Task UpdateScreenerDataAsync();
    Task<int> CountScreenerInstrumentsAsync();
    Task SelectInstrumentAsync(int index);
    Task<FinancialInstrument> GetInstrumentAsync(int index);
    Task<bool> IsOpenToastAsync();
    Task CloseToastAsync();
    Task InputTicker(string name);
    Task RefreshPage();

    // TODO: Task ChangeTimeFrame(TimeFrame timeFrame);
}
