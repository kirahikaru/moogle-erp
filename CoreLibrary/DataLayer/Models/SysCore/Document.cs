namespace DataLayer.Models.SysCore;

[Table("Document")]
public class Document : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(Document).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"document";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ReferenceType { get; set; }
    public string? ReferenceNumber { get; set; }

    public int? LinkedObjectId { get; set; }
    [StringLength(50)]
    public string? LinkedObjectRecordID { get; set; }
    public string? LinkedObjectType { get; set; }

    [MaxLength(80)]
    public string? DocumentTypeCode { get; set; }
    public string? LanguageCode { get; set; }
    public string? Filename { get; set; }
    public string? FileExtension { get; set; }
    public int? Version { get; set; }

    public byte[]? Content { get; set; }
    public int ContentLength { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}