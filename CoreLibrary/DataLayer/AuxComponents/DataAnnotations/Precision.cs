namespace DataLayer.AuxComponents.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class PrecisionAttribute : Attribute
{
    public int Precision { get; set; }
    public int Scale { get; set; }

    public PrecisionAttribute(int precision, int scale)
    {
        this.Precision = precision;
        this.Scale = scale;
    }
}