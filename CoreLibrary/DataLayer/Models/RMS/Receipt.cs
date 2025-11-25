using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.FIN;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.RMS;

[Table("[rms].[Receipt]")]
public class Receipt : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Receipt).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "receipt";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(ErrorMessage = "'Receipt Date' is required.")]
    public DateTime? ReceiptDate { get; set; }

    public int? CustomerId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Currency' is required.")]
    [MaxLength(3)]
    public string? CurrencyCode { get; set; }

    [Precision(18, 2)]
    public decimal? TotalAmount { get; set; }
    public int? TotalAmountKhr { get; set; }

    public string? DiscountType { get; set; }

    [Precision(10, 2)]
    public decimal? DiscountValue { get; set; }

    [Precision(18, 2)]
    public decimal? DiscountAmount { get; set; }

    public int? DiscountAmountKhr { get; set; }

    [Precision(18, 2)]
    public decimal? TotalItemDiscountAmount { get; set; }
    public int? TotalItemDiscountAmountKhr { get; set; }

    [Precision(18, 2)]
    public decimal? TotalOtherChargeAmount { get; set; }

    public int? TotalOtherChargeAmountKhr { get; set; }

    [Precision(18, 2)]
    public decimal? TotalTaxAmount { get; set; }

    public int? TotalTaxAmountKhr { get; set; }

    /// <summary>
    /// TotalNetPayableAmount = TotalAmount - TotalDiscountAmount
    /// </summary>
    [Precision(18, 2)]
    public decimal? TotalNetPayableAmount { get; set; }

    public int? TotalNetPayableAmountKhr { get; set; }

    /// <summary>
    /// ValidValues => GlobalConstant > ReceiptStatuses
    /// </summary>
    [MaxLength(30)]
    public string? Status { get; set; }

    [Required(ErrorMessage = "Cashier is required.")]
    public int? CashierUserId { get; set; }

    [Required(ErrorMessage = "'KHR Exchange Rate' is required.")]
    public int? KhrExchangeRateId { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalAmount => Items.Where(x => !x.IsDeleted && x.TotalAmount is not null).Sum(x => x.TotalAmount!.Value);

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalDiscountableAmount => Items.Where(x => !x.IsDeleted && x.IsEligibleForRcptLvlDiscount && x.TotalAmount is not null).Sum(x => x.TotalAmount!.Value);

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalItemDiscountAmount => Items.Where(x => !x.IsDeleted && x.DiscountAmount != null).Sum(x => x.DiscountAmount!.Value);

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalOtherChargeAmount => OtherCharges.Where(x => !x.IsDeleted && x.ChargeAmount is not null).Sum(x => x.ChargeAmount!.Value);

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalTaxableAmount => ComputedTotalAmount - ComputedDiscountAmount - ComputedTotalItemDiscountAmount - ComputedTotalTaxableOtherChargeAmount;

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalNetPayableAmount => ComputedTotalAmount - ComputedDiscountAmount - ComputedTotalItemDiscountAmount + ComputedTotalOtherChargeAmount;

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalNetPayableAmountAfterTax => ComputedTotalNetPayableAmount + ComputedTotalTaxAmount;

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedDiscountAmount
    {
        get
        {
            return DiscountType switch
            {
                ReceiptDiscountTypes.PERCENTAGE => ((ComputedTotalAmount - ComputedTotalItemDiscountAmount) * DiscountValue!.Value) * 100,
                ReceiptDiscountTypes.AMOUNT => (DiscountValue ?? 0),
                _ => 0,
            };
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalTaxAmount
    {
        get
        {
            decimal totalAmount = 0;

            foreach (RetailTaxItem taxItem in TaxItems)
                if (!taxItem.IsDeleted)
                    totalAmount += taxItem.Tax!.GetTaxAmount(ComputedTotalAmount - ComputedDiscountAmount - ComputedTotalItemDiscountAmount + ComputedTotalTaxableOtherChargeAmount);

            return totalAmount;
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalTaxableOtherChargeAmount => OtherCharges.Where(x => !x.IsDeleted && x.IsTaxable && x.ChargeAmount is not null).Sum(x => x.ChargeAmount!.Value);

	[Computed, Write(false), ReadOnly(true)]
	public string TotalAmountText => TotalAmount.HasValue ? TotalAmount!.Value.ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode!)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string TotalItemDiscountAmountText => TotalItemDiscountAmount.HasValue ? TotalItemDiscountAmount!.Value.ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode!)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string DiscountAmountText => DiscountAmount.HasValue ? DiscountAmount!.Value.ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode!)) : "-";

    [Computed]
    [Description("ignore")]
    public string TotalTaxableAmountText => ComputedTotalTaxableAmount.ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode!));

	[Computed, Write(false), ReadOnly(true)]
	public string TotalOtherChargeAmountText => TotalOtherChargeAmount.HasValue ? TotalOtherChargeAmount!.Value.ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode!)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string TotalNetPayableAmountText => TotalNetPayableAmount.HasValue ? TotalNetPayableAmount!.Value.ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode!)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string TotalNetPayableAmountAfterTaxText => TotalNetPayableAmount.HasValue ? (TotalNetPayableAmount!.Value + (TotalTaxAmount ?? 0)).ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode!)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public decimal TotalNetPayableAmountAfterTax => (TotalNetPayableAmount ?? 0) + (TotalTaxAmount ?? 0);

	[Computed, Write(false), ReadOnly(true)]
	public string TotalTaxAmountText => TotalTaxAmount.HasValue ? TotalTaxAmount!.Value.ToCurrencyText(!Currencies.HasNoDecimalDisplay(CurrencyCode!), Currencies.GetSymbol(CurrencyCode!)) : "-";
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public List<ReceiptItem> Items { get; set; }

	[Computed, Write(false)]
	public User? CashierUser { get; set; }

	[Computed, Write(false)]
	public Customer? Customer { get; set; }

	[Computed, Write(false)]
	public Currency? Currency { get; set; }

	[Computed, Write(false)]
	public ExchangeRate? KhrExchangeRate { get; set; }

	[Computed, Write(false)]
	public List<RetailOtherCharge> OtherCharges { get; set; }

	[Computed, Write(false)]
	public List<RetailTaxItem> TaxItems { get; set; }

	[Computed, Write(false)]
	public ReceiptPayment? Payment { get; set; }
    #endregion

    public Receipt() : base()
    {
        CurrencyCode = Currencies.US_USD;
        Status = ReceiptStatuses.NEW;
        Items = [];
        OtherCharges = [];
        TaxItems = [];
    }
}