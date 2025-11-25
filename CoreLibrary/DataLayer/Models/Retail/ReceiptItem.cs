using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;

[Table("[rms].[ReceiptItem]"), DisplayName("Receipt Item")]
public class ReceiptItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ReceiptItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "receipt_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	/// <summary>
	/// Receipt.ObjectCode
	/// </summary>
	public int? ReceiptId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required(ErrorMessage = "'Sequence Number' is required.")]
    public int SequenceNo { get; set; }
    
    [MaxLength(25)]
    public string? ReceiptNumber { get; set; }

    [Required(ErrorMessage = "'Item' is required to be selected.")]
    public int? ItemId { get; set; }

    [MaxLength(25)]
    public string? Barcode { get; set; }

    [MaxLength(100)]
    public string? ItemName { get; set; }

    [MaxLength(100)]
    public string? ItemNameKh { get; set; }

    [MaxLength(25)]
    public string? UnitCode { get; set; }

    [Precision(10, 2)]
    [Range(0.00, 999999999999.99, ErrorMessage = "'Quantity' must be positive.")]
    [Required(ErrorMessage ="'Quantity' is required.")]
    public decimal? Quantity { get; set; }

    [MaxLength(3)]
    [Required(ErrorMessage ="'Currency' is required.")]
    public string? CurrencyCode { get; set; }

    [Required(ErrorMessage = "'Unit Price' is required.")]
    [Precision(10, 2)]
    [Range(0.00, 999999999999.99, ErrorMessage = "'Unit Price' must be positive.")]
    public decimal? UnitPrice { get; set; }

	[Required(ErrorMessage = "'Unit Price (KHR)' is required.")]
	[Range(0, 999999999999, ErrorMessage = "'Unit Price (KHR)' must be positive.")]
	public int? UnitPriceKhr { get; set; }

    [Precision(18, 2)]
    public decimal? TotalAmount { get; set; }
    public int? TotalAmountKhr { get; set; }
    public bool IsManualDiscount { get; set; }
    public bool IsManualUnitPrice { get; set; }
    public bool IsEligibleForRcptLvlDiscount { get; set; }

    /// <summary>
    /// Valid Values > Global Constants RMS > ReceiptDiscountTypes
    /// </summary>
    public string? DiscountType { get; set; }

    [Precision(18, 2)]
    [Range(0.00, double.MaxValue, ErrorMessage = "'Discount Value' must be positive.")]
    public decimal? DiscountValue { get; set; }

    [Precision(18, 2)]
    [Range(0.00, 999999999999.99, ErrorMessage = "'Discount Amount' must be positive.")]
    public decimal? DiscountAmount { get; set; }
    public int? DiscountAmountKhr { get; set; }

    [Precision(18, 2)]
    [Range(0.00, 999999999999.99, ErrorMessage = "'Net Payable Amount' must be positive.")]
    public decimal? NetPayableAmount { get; set; }
    public int? NetPayableAmountKhr { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedUnitPrice => CurrencyCode == Currencies.CAMBODIA_KHR ? UnitPriceKhr!.Value : UnitPrice!.Value;

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalAmount => CurrencyCode == Currencies.CAMBODIA_KHR ? UnitPriceKhr!.Value * Quantity!.Value : UnitPrice!.Value * Quantity!.Value;

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedDiscountAmount
    {
        get
        {
            if (DiscountType == ReceiptDiscountTypes.PERCENTAGE)
            {
                return (ComputedTotalAmount * DiscountValue!.Value) / 100.00M; 
            }
            else if (DiscountType == ReceiptDiscountTypes.AMOUNT)
            {
                return (DiscountValue ?? 0);
            }

            return 0;
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string QuantityText
    {
        get
        {
            if (!Quantity.HasValue) return "-";

            if (Quantity % 1 > 0)
                return $"{Quantity!.Value:#,##0.00}";
            else
                return $"{Quantity!.Value:#,##0}";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string DisplayDiscountAmountText => DiscountAmount.HasValue ? DiscountAmount!.Value.ToCurrencyText(!CurrencyCode!.Is(Currencies.GetNoDecimalCurrencies()), Currencies.GetSymbol(CurrencyCode)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string UnitPriceText => UnitPrice.HasValue ? UnitPrice!.Value.ToCurrencyText(!CurrencyCode!.Is(Currencies.GetNoDecimalCurrencies()), Currencies.GetSymbol(CurrencyCode)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string TotalAmountText => TotalAmount.HasValue ? TotalAmount!.Value.ToCurrencyText(!CurrencyCode!.Is(Currencies.GetNoDecimalCurrencies()), Currencies.GetSymbol(CurrencyCode)) : "-";

    [Computed]
    [Description("ignore")]
    public string NetPayableAmountText => NetPayableAmount.HasValue ? NetPayableAmount!.Value.ToCurrencyText(!CurrencyCode!.Is(Currencies.GetNoDecimalCurrencies()), Currencies.GetSymbol(CurrencyCode)) : "-";
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public UnitOfMeasure? Unit { get; set; }

	[Computed, Write(false)]
	public Item? Item { get; set; }
    #endregion

    public ReceiptItem() : base()
    {
        IsManualUnitPrice = false;
        IsManualDiscount = false;
        IsEligibleForRcptLvlDiscount = true;
    }

    public ReceiptItem(Item item, string currencyCode) : base()
    {
        Quantity = 1;
        ItemId = item.Id;
        Item = item;

        IsEligibleForRcptLvlDiscount = true;
        ObjectCode = item.ObjectCode;
        ObjectName = item.ObjectName;
        ItemName = item.ObjectName?[..100];
        ItemNameKh = item.ObjectNameKh?[..100];
        CurrencyCode = currencyCode;
        UnitPrice = item.RetailUnitPrice;
        UnitPriceKhr = item.RetailUnitPriceKhr;
        Quantity = 1;
        Barcode = item.Barcode;

        if (UnitPrice is null)
            IsManualUnitPrice = true;
    }
}