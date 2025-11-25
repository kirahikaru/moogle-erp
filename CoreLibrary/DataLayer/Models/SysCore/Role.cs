namespace DataLayer.Models.SysCore;

[Table("Role")]
public class Role : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(Role).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"role";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? Description { get; set; }
    public int? OrgId { get; set; }
    //public string? AppCode { get; set; }
    public bool IsEnabled { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Organization? Organization { get; set; }

	[Computed, Write(false)]
	public List<Permission> Permissions { get; set; }

	[Computed, Write(false)]
	public List<RoleSysMod> AssignedSystemModules { get; set; }

	[Computed, Write(false)]
	public List<UserRole> AssignedUsers { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion

	public Role() : base()
    {
        IsEnabled = true;
		AssignedSystemModules = [];
		AssignedUsers = [];
		Permissions = [];
	}
}