namespace DataLayer.Models.SysCore;

[Table("LoginHistory"), DisplayName("Login History")]
public class LoginHistory : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public static string TableName => $"{typeof(LoginHistory).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"login_history";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	/// <summary>
	/// Username = User ID
	/// </summary>
	public string? Username { get; set; }
    public string? Action { get; set; }
    public string? TokenID { get; set; }
    public string? SessionID { get; set; }
    public string? SourceIP { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion
}