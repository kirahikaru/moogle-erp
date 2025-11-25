namespace DataLayer.AuxComponents.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class StringUnicodeAttribute : Attribute
{
    public bool IsUnicode { get; set; }

    public StringUnicodeAttribute(bool isUnicode)
    {
        this.IsUnicode = isUnicode;
    }
}