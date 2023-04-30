namespace Stupesoft.LazeScallp.Application.Configurations;

public class IndicatorOptions
{
    public const string Indicator = "indicator";
    public ImagePoint? HighLevelPosition { get; set; }
    public ImagePoint? LowLevelPosition { get; set; }
    public string? DefaultColor { get; set; }
    public string? WarningColor { get; set; }
    public string? SuccessColor { get; set; }
    public string? DangerColor { get; set; }
}
