using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("UserRole"), DisplayName("User - Role")]
public class UserRole : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(UserRole).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"user_role";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int UserId { get; set; }
	public int RoleId { get; set; }
	public string? UserUserID { get; set; }
	public string? UserName { get; set; }
	public string? RoleCode { get; set; }
	public string? RoleName { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public User? User { get; set; }

    [Computed, Write(false)]
    public Role? Role { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}