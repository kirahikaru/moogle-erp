using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;

namespace DataLayer.Models.RMS;

[Table("[rms].[InventoryCheckInItem]")]
public class InventoryCheckInItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(InventoryCheckInItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "inventory_check_in_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	public string? ObjectNameKh { get; set; }

    #region *** DATABASE FIELDS ***
    public int? InventoryCheckInId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "'Sequence No' must be > 0.")]
    [Required(ErrorMessage = "'Sequence No. is required.'")]
    public int? SequenceNo { get; set; }

    [Required(ErrorMessage = "'Item' is required.")]
    [Range(0, double.MaxValue, ErrorMessage = "ItemId must be greater than 0.")]
    public int? ItemId { get; set; }

    public int? LocationId { get; set; }

    [MaxLength(30)]
    public string? Barcode { get; set; }

    public string? Brand { get; set; }

    [MaxLength(30)]
    public string? BatchID { get; set; }

    [MaxLength(30)]
    //[Required(AllowEmptyStrings = false, ErrorMessage = "'Unit' is required.")]
    public string? UnitCode { get; set; }

    [Precision(10, 2)]
    [Required(AllowEmptyStrings =false, ErrorMessage = "'Quantity' is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage ="'Quantity' must be greater than 0.")]
    public decimal? Quantity { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Unit Price' is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "'Unit Price' must be greater than 0.")]
    public decimal? UnitPrice { get; set; }

    [Required(ErrorMessage = "'Currency' is required.")]
    public string? CurrencyCode { get; set; }

    [Precision(18, 2)]
    [Range(0.01, double.MaxValue, ErrorMessage = "'Amount' must be greater than 0.")]
    public decimal? Amount { get; set; }

    [Precision(18, 2)]
    [Range(0.01, double.MaxValue, ErrorMessage = "'Discount Amount' must be greater than 0.")]
    public decimal? DiscountAmount { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "'Payable Amount' must be greater than 0.")]
    public decimal? PayableAmount { get; set; }

    public DateTime? MfgDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? MfgCountryCode { get; set; }
    public int? ManufacturerId { get; set; }

    [MaxLength(255)]
    public string? Remark { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string QuantityText
    {
        get
        {
            if (Quantity == null)
                return "-";
            else if (Quantity.Value % 1 > 0)
                return Quantity.Value.ToString("#,##0.00");
            else
                return Quantity.Value.ToString("#,##0");
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string UnitName => Unit != null ? Unit.ObjectName.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string ManufacturerName => Manufacturer != null ? Manufacturer.ObjectName.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string LocationName => Location != null ? Location.ObjectName.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string UnitPriceText => UnitPrice.HasValue ? UnitPrice!.Value.ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string DiscountAmountText => DiscountAmount.HasValue ? DiscountAmount!.Value.ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string AmountText => Amount.HasValue ? Amount!.Value.ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string NetPayableAmountText => PayableAmount.HasValue ? PayableAmount!.Value.ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode)) : "-";
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public InventoryCheckIn? InventoryCheckIn { get; set; }

	[Computed, Write(false)]
	public Currency? Currency { get; set; }

	[Computed, Write(false)]
	public Item? Item { get; set; }

	[Computed, Write(false)]
	public Location? Location { get; set; }

	[Computed, Write(false)]
	public Manufacturer? Manufacturer { get; set; }

	[Computed, Write(false)]
	public Country? MfgCountry { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? Unit { get; set; }
    #endregion
}