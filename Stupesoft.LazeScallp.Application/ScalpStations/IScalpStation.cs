namespace Stupesoft.LazeScallp.Application.ScalpStations;

public interface IScalpStation
{
    Task<List<SSInstruments>> GetInstrumentsAsync(int count, ScalpStation.SortType sortType, string period);
}