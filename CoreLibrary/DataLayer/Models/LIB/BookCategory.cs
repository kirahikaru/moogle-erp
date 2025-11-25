using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.LIB;

[Table("[lib].[BookCategory]"), DisplayName("Book Category")]
public class BookCategory : AuditObject, IParentChildHierarchyObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.LIBRARY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(BookCategory).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "book_category";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[MaxLength(255)]
    public string? HierarchyPath { get; set; }
    public int? ParentId { get; set; }
    public string? ParentCode { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
	public BookCategory? Parent { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}