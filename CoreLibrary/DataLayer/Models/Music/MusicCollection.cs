using DataLayer.GlobalConstant;
using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Music;

[Table("[mdb].[MusicCollection]")]
public class MusicCollection : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.MUSIC;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(MusicCollection).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "music_collection";

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