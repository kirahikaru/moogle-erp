namespace DataLayer.Models.Pru.IT;

[Table("[dbo].[UserAccount]"), DisplayName("User Account")]
public class UserAccount : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(UserAccount).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "user_account";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'ID' is required.")]
    public new string? ObjectCode { get; set; }

	public string? DisplayName { get; set; }

	public string? UserID { get; set; }
	public string? UserName { get; set; }
	public string? UserType { get; set; }
	public string? UserGrouping { get; set; }
	public string? LBU { get; set; }
	public string? EmpID { get; set; }
	public string? ManagerEmpID { get; set; }
	public string? Email { get; set; }
	public bool IsEnabled { get; set; }
	public string? UserPlatform { get; set; }

	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion

	public UserAccount() : base()
    {
		IsEnabled = true;
    }
}