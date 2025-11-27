namespace DataLayer.Models.SysCore;

[Table("[dbo].[ObjectStateConfig]"), DisplayName("Object State Config")]
public class ObjectStateConfig : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public static string TableName => $"{typeof(ObjectStateConfig).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"object_state_config";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Vendor Name' is required.")]
	[MaxLength(255)]
	public new string? ObjectName { get; set; }

	public string? ObjectClassName { get; set; }
	public string? ValidSourceObjectStates { get; set; }
	public bool IsTargetUserRequired { get; set; }
	public bool IsRemarkRequired { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***

	#endregion
}
