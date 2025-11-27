using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;

namespace DataLayer.Models.RMS;

[Table("[rms].[CustPurchaseOrderItem]")]
public class CustPurchaseOrderItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(CustPurchaseOrderItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "cust_purchase_order_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ObjectNameKh { get; set; }
    public string? Barcode { get; set; }
    public int? CustomerPurchaseOrderId { get; set; }
    public int? ItemId { get; set; }
    public string? UnitCode { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "'Sequence No.' cannot be negative.")]
    public int? SequenceNo { get; set; }

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
	public UnitOfMeasure? Unit { get; set; }

	[Computed, Write(false)]
	public CustPurchaseOrder? Order { get; set; }

	[Computed, Write(false)]
	public Item? Item { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion

	public CustPurchaseOrderItem()
	{
		IsManualDiscount = false;
		IsManualUnitPrice = false;
		IsEligibleForOrderLvlDiscount = true;
	}
}