namespace Stupesoft.LazeScallp.Application.Configurations;

public class ApplicationOptions
{
    public const string SectionName = "Application";
    public int NotificationDelayTimeMin { get; set; } = 60;
    public FeaturesFlags Features{ get; set; } = new();

    public class FeaturesFlags
    {
        public bool UseColorDecoder { get; set; }
    }
}