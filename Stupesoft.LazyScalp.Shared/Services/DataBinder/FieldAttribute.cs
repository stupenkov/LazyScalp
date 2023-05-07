namespace Stupesoft.LazyScalp.Shared.Services.DataBinder;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FieldAttribute : Attribute
{
    public FieldAttribute(string fieldName)
    {
        FieldName = fieldName;
    }

    public string FieldName { get; }
}