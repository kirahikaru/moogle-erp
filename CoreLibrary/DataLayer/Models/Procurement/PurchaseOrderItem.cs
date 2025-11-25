using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Procurement;

[Table("[rms].[PurchaseOrderItem]")]
public class PurchaseOrderItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

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
	public int? ItemId { get; set; }

    public string? ItemCode { get; set; }

    [MaxLength(25)]
    public string? Barcode { get; set; }

    [MaxLength(25)]
    public string? UnitCode { get; set; }

    [Precision(10, 2)]
    public decimal? UnitPrice { get; set; }

    [Precision(18, 2)]
    public decimal? TotalAmount { get; set; }

    [MaxLength(25)]
    public string? Status { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC FIELDS ***
    #endregion
}