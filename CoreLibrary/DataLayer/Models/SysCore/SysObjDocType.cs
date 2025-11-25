namespace DataLayer.Models.SysCore;

/// <summary>
/// System Object Document Type
/// </summary>
//[Table("[dbo].[SystemObjectDocumentType]")]
[Table("SysObjDocType")]
public class SysObjDocType : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(SysObjDocType).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"sys_obj_doc_type";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? DocumentTypeCode { get; set; }
    public bool IsMandatory { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    #endregion

    #region *** LINKED OBJECT ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}