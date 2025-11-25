using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.Finance;

[Table("[dbo].[InvoiceItem]"), DisplayName("Invoice Item")]
public class InvoiceItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(InvoiceItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "invoice_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

    #region *** DATABASE FIELDS ***

    //[Required(AllowEmptyStrings = false, ErrorMessage = "'Asset ID' is required.")]
    //[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Asset ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    //[MaxLength(80)]
    //public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Item name is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
	public int? OrderNo { get; set; }
	public string? SKU { get; set; }
	public DateTime? CoverageStartDate { get; set; }
	public DateTime? CoverageEndDate { get; set; }
	[Required(ErrorMessage = "'Quantity' is required.")]
	public decimal? Quantity { get; set; }
	[Required(ErrorMessage = "'Unit Price' is required.")]
	public decimal? UnitPrice { get; set; }
	public decimal? TotalAmount { get; set; }
	public decimal? TaxAmount { get; set; }
	public int? InvoiceId { get; set; }
	public string? InvoiceCode { get; set; }
	public int? PurchaseOrderId { get; set; }
	public int? PurchaseOrderItemId { get; set; }
	public string? PurchaseOrderCode { get; set; }
	public string? Remark { get; set; }

	///// <summary>
	///// Accounting System Reference No.
	///// </summary>
	//public string? AccSysNo { get; set; }
	///// <summary>
	///// 
	///// </summary>
	//public string? GLAccountCode { get; set; }

	///// <summary>
	///// Finance Activity Code
	///// </summary>
	//public string? FinActivityCode { get; set; }

	///// <summary>
	///// Finance Project Code
	///// </summary>
	//public string? FinProjectCode { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Vendor? Vendor { get; set; }

	[Computed, Write(false)]
	public PurchaseOrder? PurchaseOrder { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion

	public InvoiceItem() : base()
    {
		
    }
}