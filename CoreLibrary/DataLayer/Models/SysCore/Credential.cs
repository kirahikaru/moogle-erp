namespace DataLayer.Models.SysCore;

[Table("Credential")]
public class Credential : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(Credential).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"credential";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? UserId { get; set; }
    public string? Username { get; set; }
    public byte[]? Hash { get; set; }
    public byte[]? Salt { get; set; }
    public int? FailedLogInAttempted { get; set; }
    public DateTime? LastSuccessfulLoginDateTime { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}