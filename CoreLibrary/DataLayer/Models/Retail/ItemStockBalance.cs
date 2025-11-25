using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;

[Table("[rms].[ItemStockBalance]")]
public class ItemStockBalance : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ItemStockBalance).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "item_stock_balance";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required]
    public int? ItemId { get; set; }
    public string? UnitCode { get; set; }

    [Required]
    public decimal TotalChkInQty { get; set; }

    [Required]
    public decimal TotalChkOutQty { get; set; }

    [Required]
    public decimal TotalAdjInQty { get; set; }

    [Required]
    public decimal TotalAdjOutQty { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYANMIC PROPERTIES ***
    #endregion

    public ItemStockBalance() : base()
    {
        
    }
}