using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

//[Table("[dbo].[RoleSystemModule]")]
[Table("RoleSysMod")]
public class RoleSysMod : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(RoleSysMod).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"role_sys_mod";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? RoleId { get; set; }
	public int? SystemModuleId { get; set; }
	public bool CanCreate { get; set; }
	public bool CanRead { get; set; }
	public bool CanUpdate { get; set; }
	public bool CanDelete { get; set; }
	public bool CanProcess { get; set; }
	public bool IsAdmin { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed]
	[Description("ignore")]
	public Role? Role { get; set; }

	[Computed]
	[Description("ignore")]
	public SystemModule? SystemModule { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion

	public RoleSysMod()
	{
		CanRead = true;
	}
}
