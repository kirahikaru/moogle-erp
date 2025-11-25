using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.RMS;

[Table("[rms].[ItemPriceHistory]")]
public class ItemPriceHistory : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ItemPriceHistory).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "item_price_history";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(ErrorMessage = "Item is reuqired to be selected.")]
    public int? ItemId { get; set; }
    public int? ItemVariationId { get; set; }

    [MaxLength(50)]
    public string? ItemCode { get; set; }

    [MaxLength(30)]
    public string? Barcode { get; set; }

    [MaxLength(3)]
    public string? CurrencyCode { get; set; }
    
    [Required(ErrorMessage ="'Retail Unit Price' is required")]

    [Range(0.00, 99999999999.99, ErrorMessage = "'Retail Unit Price' invalid format. Only positive number is allowed.")]
    public decimal? RetailUnitPrice { get; set; }

    [Range(0.00, 99999999999.99, ErrorMessage = "'Wholesale Unit Price' invalid format. Only positive number is allowed.")]
    public decimal? WholeSaleUnitPrice { get; set; }

    [Required(ErrorMessage = "'Retail Unit Price (KHR)' is required.'")]
    [Range(0, 9999999999999, ErrorMessage = "'Retail Unit Price (KRH)' invalid input. Only positive whole number is allowed")]
    public int? RetailUnitPriceKhr { get; set; }

    [Range(0, 9999999999999, ErrorMessage = "'Wholesale Unit Price (KRH)' invalid input. Only positive whole number is allowed")]
    public int? WholeSaleUnitPriceKhr { get; set; }

    [Required]
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool IsCurrentPrice { get; set; }
	#endregion

	#region ***LINKED OBJECTS***
	[Computed, Write(false)]
	public Item? Item { get; set; }
    #endregion

    #region *** DYANMIC PROPERTIES ***
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

    public ItemPriceHistory()
    {
        CurrencyCode = Currencies.US_USD;
    }
}