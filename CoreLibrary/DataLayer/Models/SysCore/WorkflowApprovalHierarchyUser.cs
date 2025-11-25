namespace DataLayer.Models.SysCore;

[Table("WorkflowApprovalHierarchyUser")]
public class WorkflowApprovalHierarchyUser : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(WorkflowApprovalHierarchyUser).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"workflow_approval_hierarchy_user";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? WorkflowApprovalHierarchyId { get; set; }
	public int? UserId { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}