using Stupesoft.LazeScallp.Application.Abstractions;

namespace Stupesoft.LazeScallp.Application.Servicies;
public class TickerSymbolConverter : ITickerSymbolConverter
{
    public string ConvertToTradingViewFeatureSymbol(string symbol)
    {
        return symbol + ".P";
    }
}
