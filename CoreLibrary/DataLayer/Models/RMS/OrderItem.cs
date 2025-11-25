using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.Finance;
using DataLayer.Models.SysCore;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.RMS;

[Table("[rms].[OrderItem]")]
public class OrderItem : WorkflowEnabledObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(OrderItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "order_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? OrderId { get; set; }

    [Required(ErrorMessage = "'Item' is required.")]
    public int? ItemId { get; set; }

	public string? Barcode { get; set; }
	public string? UnitCode { get; set; }
	public string? CurrencyCode { get; set; }

	[Precision(10, 2)]
	[Range(0.00, 999999999999.99, ErrorMessage = "'Quantity' must be positive.")]
	[Required(ErrorMessage = "'Quantity' is required.")]
	public decimal? Quantity { get; set; }

	[Required(ErrorMessage = "'Unit Price' is required.")]
	[Precision(10, 2)]
	[Range(0.00, 999999999999.99, ErrorMessage = "'Unit Price' must be positive.")]
	public decimal? UnitPrice { get; set; }

	[Range(0, double.MaxValue, ErrorMessage = "'Total Amount' cannot be negative.")]
	public decimal? TotalAmount { get; set; }

	public bool IsManualDiscount { get; set; }
	public bool IsManualUnitPrice { get; set; }
	public bool IsEligibleForOrderLvlDiscount { get; set; }

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

	[Precision(18, 2)]
	[Range(0.00, 999999999999.99, ErrorMessage = "'Net Payable Amount' must be positive.")]
	public decimal? NetPayableAmount { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Item? Item { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? Unit { get; set; }

	[Computed, Write(false)]
	public Currency? Currency { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***
	#endregion

	public OrderItem()
	{
		IsManualDiscount = false;
		IsManualUnitPrice = false;
		IsEligibleForOrderLvlDiscount = false;
	}
}