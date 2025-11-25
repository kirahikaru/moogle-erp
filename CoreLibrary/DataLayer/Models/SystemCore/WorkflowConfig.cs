using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("WorkflowConfig")]
public class WorkflowConfig : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(WorkflowConfig).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"workflow_config";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MinApprovalReqCount { get; set; }
    public double? MinApprovalReqAmount { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    public bool CheckIfApprovalRequired(int count, double amount)
    {
        if (MinApprovalReqAmount is null && MinApprovalReqAmount is null)
            return false;
        else if (MinApprovalReqCount.HasValue && count >= MinApprovalReqCount)
            return true;
        else if (MinApprovalReqAmount.HasValue && amount >= MinApprovalReqAmount)
            return true;
        else
            return false;
    }
    #endregion
}