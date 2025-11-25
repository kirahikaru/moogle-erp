using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("OrgStructType"), DisplayName("Organization Structure Type")]
public class OrgStructType : AuditObject, IParentChildHierarchyObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(OrgStructType).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"org_struct_type";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int OrgLevel { get; set; }
    public int? ParentId { get; set; }
	public string? ParentCode { get; set; }
	public string? HierarchyPath { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed]
    [Description("ignore")]
    public OrgStructType? Parent { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, ReadOnly(true)]
    public string ParentName => Parent != null ? Parent.ObjectName.NonNullValue("-") : "-";
	#endregion
}