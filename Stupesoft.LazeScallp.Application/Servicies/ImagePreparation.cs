using Microsoft.Extensions.Options;
using SkiaSharp;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.Configurations;

namespace Stupesoft.LazeScallp.Application.Servicies;

public class ImagePreparation : IImagePreparation
{
    private IOptions<ImagePreparationOptions> _imagePreparationOptions;

    public ImagePreparation(IOptions<ImagePreparationOptions> options)
    {
        _imagePreparationOptions = options;
    }

    public byte[] Crop(byte[] image)
    {
        using var inputStream = new SKManagedStream(new MemoryStream(image));
        using var original = SKBitmap.Decode(inputStream);

        using var pixmap = new SKPixmap(original.Info, original.GetPixels());
        ImagePreparationOptions options = _imagePreparationOptions.Value;
        SKRectI rectI = new SKRectI(options.Left, options.Top, options.Right, options.Bottom);

        using var subset = pixmap.ExtractSubset(rectI);
        using var data = subset.Encode(SKPngEncoderOptions.Default);
        return data.ToArray();
    }
}