using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Stupesoft.LazeScallp.Application.ScalpStations;
public class ScalpStation : IScalpStation
{
    private readonly HttpClient _httpClient;

    public ScalpStation(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public enum SortType
    {
        Volume,
        Trades,
    }

    public static ScalpStation Create(IOptions<ScalpStationOptions> options)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(options.Value.Url)
        };

        return new ScalpStation(httpClient);
    }

    public async Task<List<SSInstruments>> GetInstrumentsAsync(int count = 16, SortType sortType = SortType.Trades, string period = "15m")
    {
        string sortTypeString = SortTypeToString(sortType);
        var dataList = await _httpClient.GetFromJsonAsync<ScalpStationDataList>(
            $"?interval=15m&top={count}&order={sortTypeString}{period}&base=USDT",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return dataList!.Sorts;
    }

    private string SortTypeToString(SortType sortType) => sortType switch
    {
        SortType.Volume => "Volume",
        SortType.Trades => "Trades",
        _ => throw new NotImplementedException(),
    };
}
