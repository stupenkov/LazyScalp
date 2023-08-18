namespace Stupesoft.LazyScalp.TradingView;

public class TradingViewOptions
{
    public const string TradingView = "TradingView";
    public string? Login { get; set; }
    public string? Password { get; set; }
}

public class ScanerOptions
{
    public const string SectionName = "Scaner";
    public SelectorsOptions? Selectors { get; set; }
    public int ReloadDelayTimeSecond { get; set; }
    public bool SkipAuth { get; set; }
    public string? ChromeProfile { get; set; }
    public string? ChromeDataUserDir { get; set; }
}

public class SelectorsOptions
{
    public string? HumburgerButton { get; set; }
    public string? EnterItem { get; set; }
    public string? EmailLoginButton { get; set; }
    public string? EmailInput { get; set; }
    public string? PasswordInput { get; set; }
    public string? LoginSubmitButton { get; set; }
    public string? ChartControlButton { get; set; }
    public string? FirstChartTemplateItem { get; set; }
    public string? OpenScreenerButton { get; set; }
    public string? CloseScreenerButton { get; set; }
    public string? ScreenerInstrumentsTable { get; set; }
    public string? ScreenerUpdateDataButton { get; set; }
    public string? TogglePanelButton { get; set; }
    public string? CloseToastButton { get; set; }
    public string? TickerInput { get; set; }
}