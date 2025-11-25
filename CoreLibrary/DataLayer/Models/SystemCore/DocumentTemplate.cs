using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("DocumentTemplate"), DisplayName("Document Template")]
public class DocumentTemplate : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(DocumentTemplate).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"document_template";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required, MaxLength(80)]
    public string? DocumentTypeCode { get; set; }
    public string? LanguageCode { get; set; }
    public string? ModelObjectName { get; set; }
    public string? Filename { get; set; }
    public string? FileExtension { get; set; }
    public string? Version { get; set; }

    public byte[]? Content { get; set; }
    public int ContentLength { get; set; }
    public bool IsInUsed { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}