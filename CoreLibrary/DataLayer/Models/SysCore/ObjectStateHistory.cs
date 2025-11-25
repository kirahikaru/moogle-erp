namespace DataLayer.Models.SysCore;

[Table("ObjStateHist"), DisplayName("Object State History")]
public class ObjectStateHistory : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"ObjStateHist";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"obj_state_hist";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? ObjectId { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? ActionCode { get; set; }
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public string? Remark { get; set; }
    public int? TriggeredUserId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion
}