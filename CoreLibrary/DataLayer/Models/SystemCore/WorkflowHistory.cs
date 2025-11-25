using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("WorkflowHistory")]
public class WorkflowHistory : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(WorkflowHistory).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"workflow_history";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? LinkedObjectId { get; set; }
    public string? LinkedObjectType { get; set; }
    public string? Action { get; set; }
    public string? StartStatus { get; set; }
    public string? EndStatus { get; set; }
    public string? Remark { get; set; }
    public int? OrgStructId { get; set; }
    public int? UserId { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public OrgStruct? OrgStruct { get; set; }

    [Computed, Write(false)]
    public User? User { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    /// <summary>
    /// Display Text
    /// </summary>
    [Computed, Write(false), ReadOnly(true)]
    public string? ActionText => WorkflowActions.GetDisplayText(Action);

	/// <summary>
	/// Display Text
	/// </summary>
	[Computed, Write(false), ReadOnly(true)]
	public string? StartStatusText => WorkflowStatuses.GetDisplayText(StartStatus);

	/// <summary>
	/// Display Text
	/// </summary>
	[Computed, Write(false), ReadOnly(true)]
	public string? EndStatusText => WorkflowStatuses.GetDisplayText(EndStatus);

	[Computed, Write(false), ReadOnly(true)]
	public string? TargetUserNameWithId => User != null ? User.UserNameWithUserId : "-";
    #endregion
}