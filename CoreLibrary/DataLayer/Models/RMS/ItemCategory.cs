using DataLayer.GlobalConstant;

namespace DataLayer.Models.RMS;

/// <summary>
/// Setup table
/// </summary>
[Table("[rms].[ItemCategory]"), DisplayName("Item Category")]
public class ItemCategory : AuditObject, IParentChildHierarchyObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ItemCategory).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "item_category";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	[Required(AllowEmptyStrings = false, ErrorMessage = "'CODE' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'NAME' is required.")]
    [RegularExpression(@"^[a-zA-Z\W0-9]{0,}$", ErrorMessage = "'NAME' invalid format.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

    #region *** DATABASE FIELDS ***
    public int? ParentId { get; set; }
    public string? ParentCode { get; set; }

    [MaxLength(255)]
    public string? HierarchyPath { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
	public ItemCategory? Parent { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES
    #endregion
}