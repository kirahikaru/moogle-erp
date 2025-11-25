namespace DataLayer.Models.SysCore;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[DisplayName("Attached Image")]
[Table("[dbo].[AttachedImage]")]
public class AttachedImage : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(AttachedImage).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"attached_image";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	/// <summary>
	/// If Id of the linked object is integer value
	/// </summary>
	public int? LinkedObjectId { get; set; }
    public string? LinkedObjectType { get; set; }
	public int? OrderNo { get; set; }
	public string? ImageType { get; set; }
    public string? ImageFilename { get; set; }
    public string? ImageExtention { get; set; }

    /// <summary>
    /// Image Height in Pixel
    /// </summary>
    public int ImageHeight { get; set; }

    /// <summary>
    /// Image width in Width
    /// </summary>
    public int ImageWidth { get; set; }
    public string? ImageBase64Content { get; set; }
    public string? ImageUrl { get; set; }
    public int ImageSize { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}