using DataLayer.GlobalConstant;
using DataLayer.Models.Finance;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;

[Table("[rms].[Item]")]
public class Order : WorkflowEnabledObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Order).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "order";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Item Code' is required.")]
    [RegularExpression(@"^[A-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    #region *** DATABASE FIELDS ***
    public DateTime? OrderDateTime { get; set; }
    public int? CustomerId { get; set; }
    public int? ReceiptId { get; set; }
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

    [DefaultValue(false)]
    public bool IsConfirmed { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed]
    [Description("ignore")]
    public DeliveryOption? DeliveryOption { get; set; }

    [Computed]
    [Description("ignore")]
    public List<OrderItem> Items { get; set; }

    [Computed]
    [Description("ignore")]
    public User? AssignedUser { get; set; }

    [Computed]
    [Description("ignore")]
    public Currency? Currency { get; set; }

    [Computed]
    [Description("ignore")]
    public Customer? Customer { get; set; }

    [Computed]
    [Description("ignore")]
    public Receipt? Receipt { get; set; }
    #endregion

    #region *** DYANMIC PROPERTIES ***
    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string WorkflowStatusText => WFC_Order.GetWorkflowStatusText(WorkflowStatus);

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string TotalAmountText => TotalAmount.HasValue ? TotalAmount.ToCurrencyText(CurrencyExtension.IsCurrencyHasDecimal(CurrencyCode), Currencies.GetSymbol(CurrencyCode)) : "-";

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string TotalDiscountAmountText => TotalDiscountAmount.HasValue ? TotalDiscountAmount.ToCurrencyText(CurrencyExtension.IsCurrencyHasDecimal(CurrencyCode), Currencies.GetSymbol(CurrencyCode)) : "-";

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string TotalPayableAmountText => TotalPayableAmount.HasValue ? TotalPayableAmount.ToCurrencyText(CurrencyExtension.IsCurrencyHasDecimal(CurrencyCode), Currencies.GetSymbol(CurrencyCode)) : "-";
    #endregion

    public Order() : base()
    {
        Items = [];
    }
}