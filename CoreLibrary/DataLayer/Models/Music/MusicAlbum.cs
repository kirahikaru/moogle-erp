using DataLayer.GlobalConstant;
using DataLayer.Models.Library;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Music;

[Table("[mdb].[MusicAlbum]")]
public class MusicAlbum : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.MUSIC;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(MusicAlbum).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "music_album";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[MaxLength(150)]
    public string? Title { get; set; }

    [MaxLength(150)]
    public string? MainArtist { get; set; }

    [MaxLength(150)]
    public string? Album { get; set; }

    public int? AlbumId { get; set; }
    public int? Year { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}