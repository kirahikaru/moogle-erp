using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Use in LMS
/// </remarks>
[Table("[dbo].[Content]")]
public class Content : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(Content).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"content";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ContentCategoryCode { get; set; }

    /// <summary>
    /// Field to determine which application this content belong too.
    /// e.g. CRM mean the CommentType belong to CSCMP (mini-CRM) Application
    /// e.g. LRM mean the CommentType is applicable only to Lead Management System
    /// </summary>
    [StringLength(10)]
    public string? PruAppCode { get; set; }
    public DateTime? ScheduledStartDateTime { get; set; }
    public DateTime? ScheduledEndDateTime { get; set; }
    public int? TargetOrgStructId { get; set; }
    [StringLength(-1), StringUnicode(true)]
    public string? ContentText { get; set; }
    public bool IsHtml { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public List<Document> AttachedDocuments { get; }

	[Computed, Write(false)]
	public ContentCategory? Category { get; set; }

	[Computed, Write(false)]
	public OrgStruct? TargetOrganizationStructure { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    
    #endregion

    public Content() : base()
    {
        AttachedDocuments = [];
    }
}