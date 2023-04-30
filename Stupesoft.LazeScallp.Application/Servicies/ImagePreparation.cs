using SkiaSharp;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.Configurations;

namespace Stupesoft.LazeScallp.Application.Servicies;

public class ImagePreparation : IImagePreparation
{
    private ImagePreparationOptions _options;

    public ImagePreparation(ImagePreparationOptions options)
    {
        _options = options;
    }

    public byte[] Crop(byte[] image)
    {
        using var inputStream = new SKManagedStream(new MemoryStream(image));
        using var original = SKBitmap.Decode(inputStream);

        using var pixmap = new SKPixmap(original.Info, original.GetPixels());
        SKRectI rectI = new SKRectI(_options.Left, _options.Top, _options.Right, _options.Bottom);

        var subset = pixmap.ExtractSubset(rectI);
        using var data = subset.Encode(SKPngEncoderOptions.Default);
        return data.ToArray();
    }
}