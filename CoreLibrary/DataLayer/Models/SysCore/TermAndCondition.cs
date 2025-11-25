namespace DataLayer.Models.SysCore;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("TermAndCondition"), DisplayName("Term & Condition")]
public class TermAndCondition : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(TermAndCondition).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"term_and_condition";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? LinkedObjectId { get; set; }
    public string? LinkedRecordID { get; set; }
    public string? LinkedObjectType { get; set; }
    public string? AcceptedUserId { get; set; }
    public string? AcceptedUsername { get; set; }
    public bool IsAccepted { get; set; }
    public string? LanguageCode { get; set; }
    public string? DocumentTemplateCode { get; set; }
    public string? Content { get; set; }
    public bool IsContentHtml { get; set; }
    public string? ContentVersion { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}