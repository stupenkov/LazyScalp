using SkiaSharp;
using System.Drawing;

namespace TradingViewBot;

public class ScreenshotAnalyzer : IScreenshotAnalyzer
{
    private readonly Point _highLevel = new(2710, 110);
    private readonly Point _lowLevel = new(2710, 140);
    private SKColor _defaultColor = new(54, 54, 54);
    private SKColor _warningColor = new(241, 157, 55);
    private SKColor _successColor = new(85, 148, 74);
    private SKColor _dangerColor = new(159, 57, 52);

    public ScreenshotAnalyzer(IndicatorOptions indicatorOptions)
    {
        _highLevel = new(indicatorOptions.HighLevelPosition!.X, indicatorOptions.HighLevelPosition!.Y);
        _lowLevel = new(indicatorOptions.LowLevelPosition!.X, indicatorOptions.LowLevelPosition!.Y);
        _defaultColor = SKColor.Parse(indicatorOptions.DefaultColor);
        _warningColor = SKColor.Parse(indicatorOptions.WarningColor);
        _successColor = SKColor.Parse(indicatorOptions.SuccessColor);
        _defaultColor = SKColor.Parse(indicatorOptions.DefaultColor);
    }

    public Task<Signal> AnalyzeAsync(byte[] image)
    {
        using var inputStream = new SKManagedStream(new MemoryStream(image));
        using var original = SKBitmap.Decode(inputStream);

        var signal = new Signal(
            ColorToLevel(original.GetPixel(_highLevel.X, _highLevel.Y)),
            ColorToLevel(original.GetPixel(_lowLevel.X, _lowLevel.Y)));

        return Task.FromResult(signal);
    }

    public Task<bool> HasSignalAsync(byte[] image)
    {
        using var inputStream = new SKManagedStream(new MemoryStream(image));
        using var original = SKBitmap.Decode(inputStream);

        using (var data = original.Encode(SKEncodedImageFormat.Png, 100))
        using (var writeStream = File.OpenWrite("test.png"))
        {
            //data.SaveTo(writeStream);
        }

        return Task.FromResult(
            original.GetPixel(_highLevel.X, _highLevel.Y) == _warningColor
            || original.GetPixel(_highLevel.X, _highLevel.Y) == _successColor
            || original.GetPixel(_lowLevel.X, _lowLevel.Y) == _warningColor
            || original.GetPixel(_lowLevel.X, _lowLevel.Y) == _successColor);
    }

    public Task<bool> IndicatorLoadedAsync(byte[] image)
    {
        using var inputStream = new SKManagedStream(new MemoryStream(image));
        using var original = SKBitmap.Decode(inputStream);

        var pixel = original.GetPixel(_highLevel.X, _highLevel.Y);
        return Task.FromResult(
            pixel == _defaultColor
            || pixel == _warningColor
            || pixel == _successColor);
    }

    private int ColorToLevel(SKColor color)
    {
        if (color == _warningColor)
            return 1;
        else if (color == _successColor)
            return 2;
        else
            return 0;
    }
}
