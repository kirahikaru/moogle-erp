using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;

[Table("[rms].[ItemVariation]"), DisplayName("Item Variation")]
public class ItemVariation : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ItemVariation).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "item_variation";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Item Code' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    public int? ItemId { get; set; }
    public string? Barcode { get; set; }

    [Required(ErrorMessage = "'Variation Description' is required.")]
    public string? Desc { get; set; }
    public string? ColorDesc { get; set; }
    public string? SizeDesc { get; set; }

    [Range(0, 99999999999999999.99, ErrorMessage = "'Height' must be positive number.")]
    public decimal? Height { get; set; }
    [Range(0, 99999999999999999.99, ErrorMessage = "'Width' must be positive number.")]
    public decimal? Width { get; set; }
    [Range(0, 99999999999999999.99, ErrorMessage = "'Width' must be positive number.")]
    public decimal? Length { get; set; }

    public string? SizeUnitCode { get; set; }
    
    [Range(0, 99999999999999999.99, ErrorMessage = "'Weight' must be positive number.")]
    public decimal? Weight { get; set; }
    
    public string? WeightUnitCode { get; set; }

    public string? ThumbnailImagePath { get; set; }

    [MaxLength(3)]
    public string? CurrencyCode { get; set; }

    [Range(0.00, 99999999999.99, ErrorMessage = "'Retail Unit Price' invalid format. Only positive number is allowed.")]
    [Precision(18, 2)]
    public decimal? RetailUnitPrice { get; set; }

    [Range(0, 9999999999999, ErrorMessage = "'Retail Unit Price (KHR)' invalid input. Only positive whole number is allowed")]
    public int? RetailUnitPriceKhr { get; set; }

    [Range(0.00, 99999999999.99, ErrorMessage = "'Wholesale Unit Price' invalid format. Only positive number is allowed.")]
    public decimal? WholeSaleUnitPrice { get; set; }


    [Range(0, 9999999999999, ErrorMessage = "'Wholesale Unit Price (KHR)' invalid input. Only positive whole number is allowed")]
    public int? WholeSaleUnitPriceKhr { get; set; }

    public string? Note { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTY ***
	[Computed, Write(false), ReadOnly(true)]
	public bool HasPrice => RetailUnitPrice is not null || RetailUnitPriceKhr is not null || WholeSaleUnitPrice is not null || WholeSaleUnitPriceKhr is not null;

	[Computed, Write(false), ReadOnly(true)]
	public string WeightText => WeightUnit != null && Weight.HasValue ? $"{Weight!.Value:#,##0.#} {WeightUnit.UnitSymbol}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string DimensionText
    {
        get
        {
			if (DimensionUnit != null)
            {
                StringBuilder sb = new();

				if (Height.HasValue)
                    sb.Append($"H: {Height.Value:#,##0.#} {DimensionUnit.UnitSymbol}");

                if (Width.HasValue)
                    sb.Append((sb.Length > 0 ? " x " : "") + $"W: {Width.Value:#,##0.#} {DimensionUnit.UnitSymbol}");

                if (Length.HasValue)
                    sb.Append((sb.Length > 0 ? " x " : "") + $"L: {Length.Value:#,##0.#} {DimensionUnit.UnitSymbol}");

                return sb.ToString();
            }
            else
            {
                return "-";
            }
        }
    }

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string WholeSaleUnitPriceKhrText => WholeSaleUnitPriceKhr.HasValue ? $"KHR {WholeSaleUnitPriceKhr!.Value:#,##0}" : "-";

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string WholeSaleUnitPriceText => !string.IsNullOrEmpty(CurrencyCode) && WholeSaleUnitPrice.HasValue ? $"{CurrencyCode} {WholeSaleUnitPrice.Value:#,##0.00}" : "-";

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string RetailUnitPriceKhrText
    {
        get
        {
            if (RetailUnitPriceKhr.HasValue)
                return $"KHR {RetailUnitPriceKhr!.Value:#,##0}";
            else
                return "-";
        }
    }

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string RetailUnitPriceText
    {
        get
        {
            if (!string.IsNullOrEmpty(CurrencyCode) && RetailUnitPrice.HasValue)
                return $"{CurrencyCode} {RetailUnitPrice.Value:#,##0.00}";
            else
                return "-";
        }
    }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Item? Item { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? WeightUnit { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? DimensionUnit { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? PackageUnit { get; set; }

	[Computed, Write(false)]
	public List<ItemPriceHistory> ItemPriceHistories { get; set; }

	[Computed, Write(false)]
	public List<AttachedImage> Images { get; set; }
    #endregion

    public ItemVariation() : base()
    {
		ItemPriceHistories = [];
        Images = [];
        CurrencyCode = Currencies.US_USD;
    }
}