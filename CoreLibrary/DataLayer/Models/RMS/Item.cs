using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SysCore;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.RMS;

[Table("[rms].[Item]")]
public class Item : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Item).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Item ID' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Item ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Item Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

    #region *** DATABASE FIELDS ***
    public string? ObjectNameKh { get; set; }
    /// <summary>
    /// For limitted short name to be printed on receipt
    /// </summary>
    public string? ShortName { get; set; }
    public string? ShortNameKh { get; set; }
    public int? ItemCategoryId { get; set; }
    public string? ItemCategoryCode { get; set; }

    /// <summary>
    /// base-64 thumbnail image of the item
    /// </summary>
    public string? ThumbnailImage { get; set; }
    public string? Barcode { get; set; }
    /// <summary>
    /// Universal Product Code
    /// 12 digit unique number associated with the barcode
    /// </summary>
    [MaxLength(12)]
    public string? UPC { get; set; }

    /// <summary>
    /// International Article Number
    /// </summary>
    [MaxLength(13)]
    public string? EAN { get; set; }

    /// <summary>
    /// Internation Standard Book Number
    /// </summary>
    public string? ISBN { get; set; }

    public string? Brand { get; set; }

    [MaxLength(50)]
    public string? Model { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Manufacturing Country Code
    /// </summary>
    [MaxLength(3)]
    public string? MfgCountryCode { get; set; }

    public string? UnitCode { get; set; }

    /// <summary>
    /// Manufacturing Part Number
    /// Unambiguously identifies a part design
    /// </summary>
    public string? MPN { get; set; }
    public string? SKU { get; set; }

    public string? DimensionUnitCode { get; set; }

    [Range(0.00, 99999999999.99, ErrorMessage = "'Whole Unit Price' invalid format. Only positive number is allowed.")]
    public decimal? DimensionHeight { get; set; }

    [Range(0.00, 99999999999.99, ErrorMessage = "'Whole Unit Price' invalid format. Only positive number is allowed.")]
    public decimal? DimensionWidth { get; set; }

    [Range(0.00, 99999999999.99, ErrorMessage = "'Whole Unit Price' invalid format. Only positive number is allowed.")]
    public decimal? DimensionLength { get; set; }

    public string? WeightUnitCode { get; set; }

    [Range(0.00, 99999999999.99, ErrorMessage = "'Whole Unit Price' invalid format. Only positive number is allowed.")]
    public decimal? ItemWeight { get; set; }

    [MaxLength(3)]
    public string? CurrencyCode { get; set; }

    [Range(0.00, 99999999999.99, ErrorMessage = "'Retail Unit Price' invalid format. Only positive number is allowed.")]
    [Precision(18, 2)]
    public decimal? RetailUnitPrice { get; set; }

    [Range(0, 9999999999999, ErrorMessage = "'Retail Unit Price (KRH)' invalid input. Only positive whole number is allowed")]
    public int? RetailUnitPriceKhr { get; set; }

    [Range(0.00, 99999999999.99, ErrorMessage = "'Wholesale Unit Price' invalid format. Only positive number is allowed.")]
    public decimal? WholeSaleUnitPrice { get; set; }


    [Range(0, 9999999999999, ErrorMessage ="'Wholesale Unit Price (KRH)' invalid input. Only positive whole number is allowed")]
    public int? WholeSaleUnitPriceKhr { get; set; }

    [DataType(DataType.Url)]
    public string? InfoLink { get; set; }

    [DataType(DataType.Url)]
    public string? PurchaseLink { get; set; }

    [MaxLength(500), StringUnicode(true)]
    public string? ContentDesc { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public ItemCategory? Category { get; set; }

	[Computed, Write(false)]
	public Country? ManufacturedCountry { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? WeightUnit { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? DimensionUnit { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? ProductUnit { get; set; }

	[Computed, Write(false)]
	public List<ItemPriceHistory> ItemPriceHistories { get; set; }

	[Computed, Write(false)]
	public List<AttachedImage> Images { get; set; }

	[Computed, Write(false)]
	public List<ItemVariation> Variations { get; set; }

	[Computed, Write(false)]
	public List<ItemSupplier> Suppliers { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTY ***

	[Computed, Write(false), ReadOnly(true)]
    public string WeightText 
    { 
        get {
            if (WeightUnit != null && ItemWeight.HasValue)
                return $"{ItemWeight!.Value:#,##0.#} {WeightUnit.UnitSymbol}";
            else
                return "-";
        } 
    }

	[Computed, Write(false), ReadOnly(true)]
	public string DimensionText
    {
        get
        {
            if (DimensionUnit != null)
            {
                StringBuilder sb = new();

                if (DimensionHeight.HasValue)
                    sb.Append($"H: {DimensionHeight.Value:#,##0.#} {DimensionUnit.UnitSymbol}");

                if (DimensionWidth.HasValue)
                    sb.Append((sb.Length > 0 ? " x " : "") + $"W: {DimensionWidth.Value:#,##0.#} {DimensionUnit.UnitSymbol}");

                if (DimensionLength.HasValue)
                    sb.Append((sb.Length > 0 ? " x " : "") + $"L: {DimensionLength.Value:#,##0.#} {DimensionUnit.UnitSymbol}");

                return sb.ToString();
            }
            else
            {
                return "-";
            }
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string WholeSaleUnitPriceKhrText
    {
        get
        {
            if (WholeSaleUnitPriceKhr.HasValue)
            {
                return $"KHR {WholeSaleUnitPriceKhr!.Value:#,##0}";
            }
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string WholeSaleUnitPriceText
    {
        get
        {
            if (!string.IsNullOrEmpty(CurrencyCode) && WholeSaleUnitPrice.HasValue)
                return $"{CurrencyCode} {WholeSaleUnitPrice.Value:#,##0.00}";
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
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

	[Computed, Write(false), ReadOnly(true)]
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

    public Item() : base()
    {
        Suppliers = [];
        Variations = [];
        ItemPriceHistories = [];
        Images = [];
        CurrencyCode = Currencies.US_USD;
    }
}