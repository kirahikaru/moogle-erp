namespace DataLayer.Models.SysCore;

[Table("Permission")]
public class Permission : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(Permission).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"permission";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ObjectNameKh { get; set; }
    public string? ParentCode { get; set; }
    public string? Type { get; set; }
    public string? HierarchyPath { get; set; }
    public string? Description { get; set; }
    public int? OrganizationId { get; set; }
    public string? AppCode { get; set; }
    public bool IsEnabled { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}