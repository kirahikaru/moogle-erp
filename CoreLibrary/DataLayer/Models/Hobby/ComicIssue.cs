using DataLayer.GlobalConstant;

namespace DataLayer.Models.Hobby;

[Table("[home].[ComicIssue]")]
public class ComicIssue : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOME_INVENTORY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ComicIssue).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "comic_issue";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public string? IssueTitle { get; set; }
    public int? IssueNo { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? WriterName { get; set; }
    public string? FileName { get; set; }
    public string? FileLocationPath { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region DYANMIC PROPERTIES
    
    #endregion
}
