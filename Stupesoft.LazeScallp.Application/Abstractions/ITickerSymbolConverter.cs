namespace Stupesoft.LazeScallp.Application.Abstractions;
public interface ITickerSymbolConverter
{
    string ConvertToTradingViewFeatureSymbol(string symbol);
}
