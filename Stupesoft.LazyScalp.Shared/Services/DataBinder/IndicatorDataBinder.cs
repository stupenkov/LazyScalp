using System.Globalization;
using System.Reflection;

namespace Stupesoft.LazyScalp.Shared.Services.DataBinder;

public class IndicatorDataBinder : IIndicatorDataBinder
{
    public T Desirialize<T>(string data) where T : new()
    {
        var obj = new T();
        Type type = typeof(T);

        Dictionary<string, string> dic = data.Split(';').ToDictionary(x => x.Split('=')[0].ToLower(), x => x.Split('=')[1]);
        var propsDic = type.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            .Select(x => new { Name = GetPropName(x), PropertyInfo = x }).ToDictionary(x => x.Name, x => x.PropertyInfo);

        foreach (var valuePair in dic)
        {
            var prop = propsDic[valuePair.Key];
            object? value = Convert.ChangeType(valuePair.Value, prop.PropertyType, CultureInfo.InvariantCulture);
            prop.SetValue(obj, value ?? default);
        }

        return obj;
    }

    private static string GetPropName(PropertyInfo propertyInfo)
    {
        FieldAttribute? attr = propertyInfo.GetCustomAttribute<FieldAttribute>();
        string propName = attr != null ? attr.FieldName : propertyInfo.Name;
        return propName.ToLower();
    }
}
