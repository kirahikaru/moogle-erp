// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("ObjStatusAuditTrail"), DisplayName("Object Status Audit Trail")]
public class ObjectStatusAuditTrail : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"ObjStatusAuditTrail";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"obj_status_audit_trail";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? ObjectId { get; set; }
    public string? ActionCode { get; set; }
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public string? Remark { get; set; }
    public int? TriggeredUserId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion
}