using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore.ManyToManyLink;

[Table("[dbo].[RolePermission]")]
public class RolePermission : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(RolePermission).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"role_permission";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public int? OrganizationId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed]
    [Description("ignore")]
    public Role? Role { get; set; }

    [Computed]
    [Description("ignore")]
    public Permission? Permission { get; set; }
    #endregion
}