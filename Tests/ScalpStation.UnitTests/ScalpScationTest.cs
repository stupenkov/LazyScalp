using Microsoft.Extensions.Options;
using Moq;
using Stupesoft.LazeScallp.Application.ScalpStations;

namespace ScalpStations.IntegrationTests;

public class ScalpScationTest
{
    [Fact]
    public async Task Get_instruments_count_more_then_zero()
    {
        // Arrange
        var ssOptions = new ScalpStationOptions { Url = "https://scalpstation.com/kapi/binance/futures/kdata" };
        var ssOptionsMock = new Mock<IOptions<ScalpStationOptions>>();
        ssOptionsMock.Setup(x=>x.Value).Returns(ssOptions);
        var scalpStation = ScalpStation.Create(ssOptionsMock.Object);

        // Act
        var result = await scalpStation.GetInstrumentsAsync();

        // Assert
        Assert.True(result.Count > 0);
    }
}