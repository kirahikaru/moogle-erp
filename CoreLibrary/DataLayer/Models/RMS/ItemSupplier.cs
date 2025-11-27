using DataLayer.GlobalConstant;

namespace DataLayer.Models.RMS;

[Table("[rms].[ItemSupplier]")]
public class ItemSupplier : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ItemSupplier).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "item_supplier";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(ErrorMessage = "'Item' is required.")]
    [Range(1, int.MaxValue, MinimumIsExclusive = true, ErrorMessage = "'Item' is not selected")]
    public int? ItemId { get; set; }

    [Required(ErrorMessage = "'Supplier' is required.")]
    [Range(1, int.MaxValue, MinimumIsExclusive = true, ErrorMessage = "'Supplier' is not selected.")]
    public int? SupplierId { get; set; }
    public string? ItemCode { get; set; }
    public string? SupplierCode { get; set; }

    [Range(0.00, double.MaxValue, MinimumIsExclusive = true, ErrorMessage = "'Unit Price' can only be positive value.")]
    public decimal? UnitPrice { get; set; }
    public string? UnitPriceCurrencyCode { get; set; }

    [Range(0.00, double.MaxValue, MinimumIsExclusive = true, ErrorMessage = "'Unit Price (USD)' can only be positive value.")]
    public decimal? UnitPriceUsd { get; set; }

    [Range(0, int.MaxValue, MinimumIsExclusive = true, ErrorMessage = "'Unit Price (KHR)' can only be positive value.")]
    public int? UnitPriceKhr { get; set; }
    public string? PurhcaseLink { get; set; }
    public bool IsCurrent { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTY ***
	[Computed, Write(false), ReadOnly(false)]
	public string UnitPriceKhrText => "KHR " + (UnitPriceKhr != null ? UnitPriceKhr!.Value.ToString("#,##0") : "-" );

	[Computed, Write(false), ReadOnly(false)]
	public string UnitPriceUsdText => "US$ " + (UnitPriceUsd != null ? UnitPriceUsd!.Value.ToString("#,##0.00") : "-");

	[Computed, Write(false), ReadOnly(false)]
	public string UnitPriceText => !string.IsNullOrEmpty(UnitPriceCurrencyCode) && this.UnitPrice is not null ? UnitPriceCurrencyCode! + " " + this.UnitPrice!.Value.ToString("#,##0.00") : "-";
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Item? Item { get; set; }

	[Computed, Write(false)]
    public Supplier? Supplier { get; set; }
    #endregion

    public ItemSupplier()
    {
        IsCurrent = true;
    }
}