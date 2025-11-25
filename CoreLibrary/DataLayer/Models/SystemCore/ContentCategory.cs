using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("ContentCategory"), DisplayName("Content Category")]
public class ContentCategory : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(ContentCategory).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"content_category";

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