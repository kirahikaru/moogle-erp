namespace DataLayer.Models.SysCore;

[Table("WorkflowApprovalHierarchyRole")]
public class WorkflowApprovalHierarchyRole : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(WorkflowApprovalHierarchyRole).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"workflow_approval_hierarchy_role";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? WorkflowApprovalHierarchyId { get; set; }
    public int? RoleId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    
    #endregion
}