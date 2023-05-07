using Stupesoft.LazyScalp.Shared.Services.DataBinder;

namespace Stupesoft.LazyScalp.Shared.Models;

public class IndicatorValues
{
    [Field("lLevel")]
    public double LowLevel { get; set; }
    [Field("hLevel")]
    public double HighLevel { get; set; }
    [Field("natr")]
    public double NATR { get; set; }
    [Field("corr")]
    public double Correlation { get; set; }
}
