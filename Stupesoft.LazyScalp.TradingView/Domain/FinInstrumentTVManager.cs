namespace Stupesoft.LazyScalp.TradingView.Domain;
internal class FinInstrumentTVManager : IFinInstrumentTVManager
{
    public FinInstrumentTV Create(byte[] image, string ticker)
    {
        if (string.IsNullOrEmpty(ticker))
            throw new ArgumentException($"'{nameof(ticker)}' cannot be null or empty.", nameof(ticker));

        return new FinInstrumentTV
        {
            Id = 0,
            ChartImage = image,
            Ticker = ticker,
        };
    }
}
