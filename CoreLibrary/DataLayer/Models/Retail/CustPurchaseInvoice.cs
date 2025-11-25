using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.Finance;
using DataLayer.Models.SystemCore;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;

[Table("[rms].[CustPurchaseInvoice]"), DisplayName("Customer Purchase Invoice")]
public class CustPurchaseInvoice : WorkflowEnabledObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "CustPurchaseInv";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "cust_purchase_inv";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Item Code' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    #region *** DATABASE FIELDS ***
    public DateTime? InvoiceDate { get; set; }
    public int? KhrExchangeRateId { get; set; }
    public int? CustomerId { get; set; }
    public int? CustomerPurchaseOrderId { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? TotalDiscountAmount { get; set; }
    
    public decimal? OtherChargeAmount { get; set; }
    public decimal? TotalTaxAmount { get; set; }
    
    public decimal? TotalPayableAmount { get; set; }
    public decimal? DepositAmount { get; set; }

    public int? TotalAmountKhr { get; set; }
    public int? TotalDiscountAmountKhr { get; set; }
    public int? OtherChargeAmountKhr { get; set; }
    public int? TotalTaxAmountKhr { get; set; }
    public int? TotalPayableAmountKhr { get; set; }
    public int? DepositAmountKhr { get; set; }
    public string? Note { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed]
    [Description("ignore")]
    public Customer? Customer { get; set; }

    [Computed]
    [Description("ignore")]
    public Currency? Currency { get; set; }

    [Computed]
    [Description("ignore")]
    public CustPurchaseOrder? PurchaseOrder { get; set; }

    [Computed]
    [Description("ignore")]
    public ExchangeRate? KhrExchangeRate { get; set; }

    [Computed]
    [Description("ignore")]
    public List<CustPurchaseInvItem> Items { get; set; }

    [Computed]
    [Description("ignore")]
    public List<CustPurchaseInvPayment> Payments { get; set; }

    [Computed]
    [Description("ignore")]
    public List<RetailOtherCharge> OtherCharges { get; set; }

    [Computed]
    [Description("ignore")]
    public List<RetailTaxItem> TaxItems { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false), ReadOnly(true)]
    public string TotalAmountText
    {
        get
        {
            if (!string.IsNullOrEmpty(CurrencyCode) && TotalAmount.HasValue)
                return $"{CurrencyCode} {TotalAmount.Value:#,##0.00}";
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string TotalDiscountAmountText
    {
        get
        {
            if (!string.IsNullOrEmpty(CurrencyCode) && TotalDiscountAmount.HasValue)
                return $"{CurrencyCode} {TotalDiscountAmount.Value:#,##0.00}";
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string OtherChargeAmountText
    {
        get
        {
            if (!string.IsNullOrEmpty(CurrencyCode) && OtherChargeAmount.HasValue)
                return $"{CurrencyCode} {OtherChargeAmount.Value:#,##0.00}";
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string TotalTaxAmountText
    {
        get
        {
            if (!string.IsNullOrEmpty(CurrencyCode) && TotalTaxAmount.HasValue)
                return $"{CurrencyCode} {TotalTaxAmount.Value:#,##0.00}";
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string TotalPayableAmountText
    {
        get
        {
            if (!string.IsNullOrEmpty(CurrencyCode) && TotalPayableAmount.HasValue)
                return $"{CurrencyCode} {TotalPayableAmount.Value:#,##0.00}";
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string DepositAmountText
    {
        get
        {
            if (!string.IsNullOrEmpty(CurrencyCode) && DepositAmount.HasValue)
                return $"{CurrencyCode} {DepositAmount.Value:#,##0.00}";
            else
                return "-";
        }
    }
    #endregion

    public CustPurchaseInvoice() : base()
    {
        Items = [];
        Payments = [];
        OtherCharges = [];
        TaxItems = [];
    }
}