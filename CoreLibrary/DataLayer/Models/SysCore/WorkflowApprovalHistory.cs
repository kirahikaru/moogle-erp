namespace DataLayer.Models.SysCore;

[Table("WorkflowApprovalHistory")]
public class WorkflowApprovalHistory : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(WorkflowApprovalHistory).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"workflow_approval_history";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? LinkedObjectId { get; set; }
    public int? LinkedObjectType { get; set; }
    public int? CurApprovalLvl { get; set; }
    public double? PendingApprovalAmount { get; set; }
    public double? ApprovedAmount { get; set; }
    public int? LastApproveUserId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}