namespace DataLayer.Models.SystemCore.NonPersistent;

public class DatabaseField
{
    public string? TableCatalog { get; set; }
    public string? TableSchema { get; set; }
    public string? TableName { get; set; }
    public string? ColumnName { get; set; }
    public int? OrdinalPosition { get; set; }
    public string? ColumnDefault { get; set; }
    public string? NullableFlag { get; set; }
    public string? DataType { get; set; }
    public int? MaxCharLength { get; set; }
    public int? OctetCharLength { get; set; }
    public int? NumericPrecision { get; set; }
    public int? NumericScale { get; set; }
    public string? CharSetName { get; set; }
    public string? CollationCatalog { get; set; }
    public string? CollationSchema { get; set; }
    public string? CollationName { get; set; }

}