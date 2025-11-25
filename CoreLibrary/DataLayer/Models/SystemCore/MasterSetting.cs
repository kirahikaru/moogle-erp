using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("MasterSetting")]
public class MasterSetting : AuditObject
{
    public DateTime? BusinessDate { get; set; }

	[Computed, ReadOnly(true), Write(false)]
	public static string TableName => $"{typeof(MasterSetting).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"master_setting";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYNAMIC PROPERTIES ***

	#endregion
}