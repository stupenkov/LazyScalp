using Stupesoft.LazyScalp.Shared.Models;

namespace Stupesoft.LazyScalp.Shared.Configurations;

public class IndicatorOptions
{
    public const string SectionName = "indicator";
    public ImagePoint? HighLevelPosition { get; set; }
    public ImagePoint? LowLevelPosition { get; set; }
    public string? DefaultColor { get; set; }
    public string? WarningColor { get; set; }
    public string? SuccessColor { get; set; }
    public string? DangerColor { get; set; }
    public ImagePoint? CodeColorIndicatorTarget { get; set; }
    public int CodeColorIndicatorSideSize { get; set; } 
}
