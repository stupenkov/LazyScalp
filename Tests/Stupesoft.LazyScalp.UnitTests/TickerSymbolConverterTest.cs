using Stupesoft.LazeScallp.Application.Servicies;

namespace Stupesoft.LazyScalp.UnitTests;

public class TickerSymbolConverterTest
{
    [Fact]
    public void Should_symbol_with_P()
    {
        // Arrange 
        var simpleSymbol = "BTCUSDT";
        var converter = new TickerSymbolConverter();

        // Act
        var result = converter.ConvertToTradingViewFeatureSymbol(simpleSymbol);

        // Assert
        Assert.Equal("BTCUSDT.P", result);
    }
}