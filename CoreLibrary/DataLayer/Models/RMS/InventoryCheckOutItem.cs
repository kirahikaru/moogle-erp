using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.RMS;

[Table("[rms].[InventoryCheckOutItem]")]
public class InventoryCheckOutItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(InventoryCheckOutItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "inventory_check_out_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(ErrorMessage = "'Sequence No.' is required.")]
    public int? SequenceNo { get; set; }
    public string? ObjectNameKh { get; set; }
    public int? InventoryCheckOutId { get; set; }
    public int? ItemId { get; set; }
    public int? LocationId { get; set; }
    public string? Barcode { get; set; }

    [MaxLength(30)]
    public string? BatchID { get; set; }

    [MaxLength(30)]
    public string? UnitCode { get; set; }

    [Precision(10, 2)]
    public decimal? Quantity { get; set; }

    public DateTime? ExpiryDate { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string LocationName => Location != null ? Location.ObjectName.NonNullValue("-") : "-";

	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public InventoryCheckOut? CheckOut { get; set; }

	[Computed, Write(false)]
	public Item? Item { get; set; }

	[Computed, Write(false)]
	public Location? Location { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? Unit { get; set; }
    #endregion
}