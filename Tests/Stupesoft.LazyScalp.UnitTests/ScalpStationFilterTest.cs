using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.ScalpStations;
using Stupesoft.LazeScallp.Application.Servicies;
using Stupesoft.LazyScalp.Domain.FinInstruments;

namespace Stupesoft.LazyScalp.UnitTests;

public class ScalpStationFilterTest
{
    [Theory]
    [InlineData("ETHUSDT.P", true)]
    [InlineData("ARBUSDT.P", false)]
    public async Task Filter_success(string filteredSymbol, bool expectedResult)
    {
        // Arrange 
        var instrumentList = new List<SSInstruments>
        {
            new SSInstruments {Symbol = "BTCUSDT" },
            new SSInstruments {Symbol = "ETHUSDT" },
        };

        var scalpStationOptions = new ScalpStationOptions
        {
            Filters =
            {
                new ScalpStationOptions.FilterOptions{Period = "", SortType = ScalpStation.SortType.Trades, Top = 1}
            }
        };

        var scalpStationOptionsMock = new Mock<IOptions<ScalpStationOptions>>();
        scalpStationOptionsMock.Setup(x => x.Value).Returns(scalpStationOptions);

        var scalpStationMock = new Mock<IScalpStation>();
        scalpStationMock.Setup(x => x.GetInstrumentsAsync(It.IsAny<int>(), It.IsAny<ScalpStation.SortType>(), It.IsAny<string>()))
            .ReturnsAsync(instrumentList);

        var converterMock = new Mock<ITickerSymbolConverter>();
        converterMock.Setup(x => x.ConvertToTradingViewFeatureSymbol(It.Is<string>(x => instrumentList.Any(i => i.Symbol == x))))
            .Returns<string>(x => x + ".P");

        var loggerMock = new Mock<ILogger<ScalpStationFilter>>();

        // Act
        var filter = new ScalpStationFilter(scalpStationMock.Object, converterMock.Object, scalpStationOptionsMock.Object, loggerMock.Object);
        await filter.UpdateAsync();
        var result = await filter.FilterAsync(new FinInstrument { Ticker = filteredSymbol });

        // Assert
        Assert.Equal(expectedResult, result);
    }
}