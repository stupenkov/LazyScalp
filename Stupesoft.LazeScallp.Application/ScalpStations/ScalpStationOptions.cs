using static Stupesoft.LazeScallp.Application.ScalpStations.ScalpStation;

namespace Stupesoft.LazeScallp.Application.ScalpStations;

public class ScalpStationOptions
{
    public const string SectionName = "ScalpStation";
    public string Url { get; set; } = string.Empty;
    public List<FilterOptions> Filters { get; set; } = new List<FilterOptions>();

    public class FilterOptions
    {
        public int Top { get; set; }
        public SortType SortType { get; set; }
        public string Period { get; set; } = string.Empty;
    }
}