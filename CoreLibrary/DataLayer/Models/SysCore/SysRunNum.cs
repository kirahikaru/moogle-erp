namespace DataLayer.Models.SysCore;

//[Table("[dbo].[SystemRunningNumber]")]
[Table("SysRunNum")]
public class SysRunNum : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(SysRunNum).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"sys_run_num";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int Number { get; set; }
    public bool IsLocked { get; set; }
    public string? LockedByUserId { get; set; }
    public int? LinkedObjectId { get; set; }
    public string? LinkedObjectType { get; set; }
    #endregion

    #region *** LINKED OBJECT ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}