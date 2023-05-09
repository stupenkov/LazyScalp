using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Stupesoft.LazyScalp.Shared.Abstractions;
using Stupesoft.LazyScalp.Shared.Configurations;
using Stupesoft.LazyScalp.Shared.Models;
using Stupesoft.LazyScalp.Shared.Services.DataBinder;
using System.Drawing;
using System.Text;

namespace Stupesoft.LazyScalp.Shared.Services;

public class ScreenshotAnalyzer : IScreenshotAnalyzer
{
    private readonly Point _highLevel = new(2710, 110);
    private readonly Point _lowLevel = new(2710, 140);
    private readonly IOptions<IndicatorOptions> _indicatorOptions;
    private readonly ILogger<ScreenshotAnalyzer> _logger;
    private readonly IIndicatorDataBinder _indicatorDataSerializer;
    private SKColor _defaultColor = new(54, 54, 54);
    private SKColor _warningColor = new(241, 157, 55);
    private SKColor _successColor = new(85, 148, 74);
    private SKColor _dangerColor = new(159, 57, 52);

    public ScreenshotAnalyzer(IOptions<IndicatorOptions> indicatorOptions, ILogger<ScreenshotAnalyzer> logger, IIndicatorDataBinder indicatorDataSerializer)
    {
        IndicatorOptions options = indicatorOptions.Value;
        _highLevel = new(options.HighLevelPosition!.X, options.HighLevelPosition!.Y);
        _lowLevel = new(options.LowLevelPosition!.X, options.LowLevelPosition!.Y);
        _defaultColor = SKColor.Parse(options.DefaultColor);
        _warningColor = SKColor.Parse(options.WarningColor);
        _successColor = SKColor.Parse(options.SuccessColor);
        _defaultColor = SKColor.Parse(options.DefaultColor);
        _indicatorOptions = indicatorOptions;
        _logger = logger;
        _indicatorDataSerializer = indicatorDataSerializer;
    }

    public Task<HLevelSignal> AnalyzeAsync(byte[] image)
    {
        using var inputStream = new SKManagedStream(new MemoryStream(image));
        using var original = SKBitmap.Decode(inputStream);

        var signal = new HLevelSignal(
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

    public Task<IndicatorValues> GetIndicatorValues(byte[] image)
    {
        int cellSideCount = _indicatorOptions.Value.CodeColorIndicatorSideSize;
        Point target = new Point(_indicatorOptions.Value.CodeColorIndicatorTarget!.X, _indicatorOptions.Value.CodeColorIndicatorTarget!.Y);

        using var original = SKBitmap.Decode(image);
        SKRectI frameRec = Util.FindBoundaries(target, original);
        if (frameRec.Left < 1 || frameRec.Top < 1 || frameRec.Height < 1 || frameRec.Width < 1)
        {
            throw new Exception("No color code indicator found!");
        }

        Size cellRec = Util.CalculateCellSize(frameRec, cellSideCount);
        PointF[] cellBreakpoints = Util.CreateCellBrakepoints(cellRec);
        using SKBitmap indicatorImage = Util.CutBitmap(original, frameRec);
        List<SKColor> colors = Util.GetColorsCode(indicatorImage, cellRec, cellBreakpoints, cellSideCount);
        string decoded = Util.DecodeColors(colors);
        _logger.LogInformation("Decode indicator values: {decoded}", decoded);
        IndicatorValues indicator = _indicatorDataSerializer.Desirialize<IndicatorValues>(decoded);
        return Task.FromResult(indicator);
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

    public static class Util
    {
        public static SKBitmap CutBitmap(SKBitmap source, SKRectI rect)
        {
            using var pixmap = new SKPixmap(source.Info, source.GetPixels());
            var subset = pixmap.ExtractSubset(rect);
            SKBitmap croped = SKBitmap.FromImage(SKImage.FromPixels(subset));
            return croped;
        }

        public static string DecodeColors(List<SKColor> colors)
        {
            var bytes = new List<byte>();
            foreach (var color in colors)
            {
                bytes.Add(color.Red);
                bytes.Add(color.Green);
                bytes.Add(color.Blue);
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        public static List<SKColor> GetColorsCode(SKBitmap image, Size cellSize, PointF[] cellBreakpoints, int cellSideCount)
        {
            var colors = new List<SKColor>();
            for (int y = 0; y < cellSideCount; y++)
            {
                for (int x = 0; x < cellSideCount; x++)
                {
                    var points = cellBreakpoints.Select(point => new Point((int)(point.X + x * cellSize.Width), (int)(point.Y + y * cellSize.Height))).ToArray();
                    List<SKColor> brakePointsColors = points.Select(p => image.GetPixel(p.X, p.Y)).ToList();
                    var color = brakePointsColors.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).First();
                    colors.Add(color);
                }
            }

            return colors;
        }

        public static PointF[] CreateCellBrakepoints(Size cellRec) => new PointF[] {
            new PointF(cellRec.Width / 2, cellRec.Height * 0.25f),
            new PointF(cellRec.Width / 2, cellRec.Height * 0.75f),
            new PointF(cellRec.Width / 2, cellRec.Height * 0.5f)
        };

        public static Size CalculateCellSize(SKRectI frameRec, int sideSize)
        {
            return new Size(frameRec.Width / sideSize, frameRec.Height / sideSize);
        }

        public static SKRectI FindBoundaries(Point target, SKBitmap original)
        {
            SKRectI imageRect = original.Info.Rect;

            var colorPatternAnalyzer = new ColorPatternAnalyzer();
            int right = 0;
            int left = 0;
            int top = 0;
            int bottom = 0;

            int x = target.X;
            int y = target.Y;
            while (x < imageRect.Width)
            {
                SKColor currColor = original.GetPixel(x, y);
                if (currColor == new SKColor(255, 255, 255))
                {

                }
                bool hasPattern = colorPatternAnalyzer.Add(currColor);
                if (hasPattern)
                {
                    right = x - 1;
                    break;
                }

                x++;
            }

            x = target.X;
            colorPatternAnalyzer.Reset();
            while (x > 0)
            {
                SKColor currColor = original.GetPixel(x, y);
                bool hasPattern = colorPatternAnalyzer.Add(currColor);
                if (hasPattern)
                {
                    left = x + 2;
                    break;
                }

                x--;
            }

            x = target.X;
            colorPatternAnalyzer.Reset();
            while (y < imageRect.Height)
            {
                SKColor currColor = original.GetPixel(x, y);
                bool hasPattern = colorPatternAnalyzer.Add(currColor);
                if (hasPattern)
                {
                    bottom = y - 1;
                    break;
                }

                y++;
            }

            y = target.Y;
            colorPatternAnalyzer.Reset();
            while (y > 0)
            {
                SKColor currColor = original.GetPixel(x, y);
                bool hasPattern = colorPatternAnalyzer.Add(currColor);
                if (hasPattern)
                {
                    top = y + 2;
                    break;
                }

                y--;
            }

            return new SKRectI(left, top, right, bottom);
        }
    }

    public class ColorPatternAnalyzer
    {
        private readonly List<SKColor> _colors = new List<SKColor>();
        private readonly int _maxSize = 3;
        private readonly SKColor _targetColor = new SKColor(255, 255, 255);

        public bool Add(SKColor color)
        {
            _colors.Add(color);
            if (_colors.Count > _maxSize)
            {
                _colors.RemoveAt(0);
            }

            if (_colors.Count < 3)
                return false;

            return _colors[0] != _targetColor && _colors[1] == _targetColor && _colors[2] != _targetColor;
        }

        public void Reset()
        {
            _colors.Clear();
        }
    }
}
