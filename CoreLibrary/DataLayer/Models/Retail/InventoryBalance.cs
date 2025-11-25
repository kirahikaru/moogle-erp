using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;

[Table("[rms].[InventoryBalance]")]
public class InventoryBalance : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(InventoryBalance).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "inventory_balance";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? ItemId { get; set; }

    /// <summary>
    /// Quantity in stock
    /// </summary>
    [Precision(10, 2)]
    public decimal? StockQty { get; set; }

    /// <summary>
    /// Quantity on shelf i.e. checked out from stock
    /// </summary>
    [Precision(10, 2)]
    public decimal? ShelfQty { get; set; }
    #endregion
}