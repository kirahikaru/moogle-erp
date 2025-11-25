using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Music;

[Table("[mdb].[MusicArtist]")]
public class MusicArtist : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.MUSIC;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(MusicArtist).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "music_artist";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[MaxLength(150)]
    public string? FirstName { get; set; }

    [MaxLength(150)]
    public string? LastName { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}