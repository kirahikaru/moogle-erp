using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("WorkflowApprovalHierarchy"), DisplayName("Workflow Approval Hierarchy")]
public class WorkflowApprovalHierarchy : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(WorkflowApprovalHierarchy).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"workflow_approval_hierarchy";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? WorkflowConfigId { get; set; }
    public int? ApprovalLevel { get; set; }
    public int? MinApprovalCount { get; set; }
    public int? MaxApprovalCount { get; set; }
    public double? MinApprovalAmount { get; set; }
	public double? MaxApprovalAmount { get; set; }
    public bool IsFinal { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    
    #endregion
}