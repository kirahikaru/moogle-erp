using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.HomeInventory;

[Table("[home].[OwnedItemCategory]"), DisplayName("My Item Category")]
public class OwnedItemCategory : AuditObject, IParentChildHierarchyObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOME_INVENTORY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(OwnedItemCategory).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "owned_item_category";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'CODE' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Item 'NAME' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

	public int? ParentId { get; set; }
	public string? ParentCode { get; set; }
	public string? HierarchyPath { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public OwnedItemCategory? Parent { get; set; }
    #endregion

    #region *** DYANMIC PROPERTIES ***

    #endregion
}