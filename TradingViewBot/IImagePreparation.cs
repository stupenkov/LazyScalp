using System.Drawing;

namespace TradingViewBot;

public interface IImagePreparation
{
    Image Crop(Image image);
}
