using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.RMS;

[Table("[rms].[InventoryCheckOut]"), DisplayName("Inventory Check-Out")]
public class InventoryCheckOut : WorkflowEnabledObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(InventoryCheckOut).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "inventory_check_out";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public DateTime? CheckOutDateTime { get; set; }
    public string? CheckOutSummary { get; set; }
    public string? Remark { get; set; }

    public int? RequestorUserId { get; set; }
    public int? ApprovedUserId { get; set; }
	#endregion

	#region *** LINKED OBJECT ***
	[Computed, Write(false)]
	public User? AssignedUser { get; set; }

	[Computed, Write(false)]
	public User? RequestorUser { get; set; }

	[Computed, Write(false)]
	public User? ApprovedUser { get; set; }

	[Computed, Write(false)]
	public List<InventoryCheckOutItem> Items { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string WorkflowStatusText => WorkflowStatuses.GetDisplayText(WorkflowStatus);

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedQuantity
    {
        get
        {
            decimal total = 0;

            foreach (InventoryCheckOutItem item in Items)
                if (!item.IsDeleted)
                    total += item.Quantity!.Value;

            return total;
        }
    }
    #endregion

    public InventoryCheckOut()
    {
        Items = [];
    }
}