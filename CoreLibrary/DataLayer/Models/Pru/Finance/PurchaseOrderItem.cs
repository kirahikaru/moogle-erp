using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.Finance;

[Table("[dbo].[PurchaseOrderItem]"), DisplayName("Purchase Order Item")]
public class PurchaseOrderItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(PurchaseOrderItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "purchase_order_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "Item Name is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'No.' is required.")]
	public int? OrderNo { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "'Unit Price' is required.")]
	[Range(0.00, 999999999999.99, ErrorMessage = "'Unit Price' must be positive number.")]
	public decimal? UnitPrice { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Quantity' is required.")]
    [Range(0.00, 999999999999.99, ErrorMessage = "'Quantity' must be positive number.")]
	public decimal? Quantity { get; set; }
	public decimal? TotalAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
	public string? Remark { get; set; }
    public int? PurchaseOrderId { get; set; }
    public string? PurchaseOrderCode { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion

    public PurchaseOrderItem() : base()
    {
		
    }
}