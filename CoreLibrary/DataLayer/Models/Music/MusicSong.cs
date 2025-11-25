using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Music;
[Table("[mdb].[MusicSong]")]
public class MusicSong : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.MUSIC;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(MusicSong).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "music_song";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[MaxLength(255), StringUnicode(true)]
    public string? Title { get; set; }
    [MaxLength(150), StringUnicode(true)]
    public string? MainArtist { get; set; }
    [MaxLength(150), StringUnicode(true)]
    public string? FeaturedArtist { get; set; }

    [MaxLength(150), StringUnicode(true)]
    public string? Album { get; set; }
    public int? AlbumId { get; set; }
    public uint Year { get; set; }
    public int? BitRate { get; set; }
    public int? TrackNo { get; set; }
    [MaxLength(255)]
    public string? FilePath { get; set; }
    public int? FileSize { get; set; }

    [MaxLength(50)]
    public string? FileSizeUnit { get; set; }
    public bool IsCover { get; set; }
    #endregion
    
    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}