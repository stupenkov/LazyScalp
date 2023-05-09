namespace Stupesoft.LazyScalp.Shared.Services.DataBinder;

public interface IIndicatorDataBinder
{
    T Desirialize<T>(string data) where T : new();
}