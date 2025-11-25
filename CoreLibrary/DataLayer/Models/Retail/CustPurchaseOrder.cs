using DataLayer.GlobalConstant;
using DataLayer.Models.Finance;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;

[Table("[rms].[CustomerPurchaseOrder]"), DisplayName("Customer Purchase Order")]
public class CustPurchaseOrder : WorkflowEnabledObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(CustPurchaseOrder).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "cust_purchase_order";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Purchase Order ID' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Purchase Order ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }
    
    #region *** DATABASE FIELDS ***
    public DateTime? OrderDateTime { get; set; }
    [Required(ErrorMessage = "'Customer' is required to be selected.")]
    public int? CustomerId { get; set; }
    public int? DeliveryOptionId { get; set; }
    public string? CurrencyCode { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "'Total Amount' cannot be negative.")]
    public decimal? TotalAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "'Total Discount Amount' cannot be negative.")]
    public decimal? TotalDiscountAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "'Total Payable Amount' cannot be negative.")]
    public decimal? TotalPayableAmount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "'Total Amount (KHR)' cannot be negative.")]
    public int? TotalAmountKhr { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "'Total Discount Amount (KHR)' cannot be negative.")]
    public int? TotalDiscountAmountKhr { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "'Total Payable Amount (KHR)' cannot be negative.")]
    public int? TotalPayableAmountKhr { get; set; }
    public int? CustPurchaseInvId { get; set; }
    [DefaultValue(false)]
    public bool IsConfirmed { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Customer? Customer { get; set; }

	[Computed, Write(false)]
	public User? AssignedUser { get; set; }

	[Computed, Write(false)]
	public Currency? Currency { get; set; }

	[Computed, Write(false)]
	public DeliveryOption? DeliveryOption { get; set; }

	[Computed, Write(false)]
	public CustPurchaseInvoice? Invoice { get; set; }

	[Computed, Write(false)]
	public List<CustPurchaseOrderItem> Items { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string TotalAmountText => TotalAmount.HasValue ? TotalAmount.ToCurrencyText(CurrencyExtension.IsCurrencyHasDecimal(CurrencyCode), Currencies.GetSymbol(CurrencyCode)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string TotalDiscountAmountText => TotalDiscountAmount.HasValue ? TotalDiscountAmount.ToCurrencyText(CurrencyExtension.IsCurrencyHasDecimal(CurrencyCode), Currencies.GetSymbol(CurrencyCode)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string TotalPayableAmountText => TotalPayableAmount.HasValue ? TotalPayableAmount.ToCurrencyText(CurrencyExtension.IsCurrencyHasDecimal(CurrencyCode), Currencies.GetSymbol(CurrencyCode)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string WorkflowStatusText => WorkflowStatuses.GetDisplayText(WorkflowStatus);
    #endregion

    public CustPurchaseOrder() : base()
    {
        Items = [];
        IsConfirmed = false;
    }
}