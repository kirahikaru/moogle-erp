using DataLayer.GlobalConstant;

namespace DataLayer.Models.LIB;

[Table("[lib].[BookGenre]"), DisplayName("Genre")]
public class BookGenre : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.LIBRARY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(BookGenre).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "book_genre";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'CODE' is required.")]
	[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	[MaxLength(80)]
	public new string? ObjectCode { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "Item 'NAME' is required.")]
	[MaxLength(255)]
	public new string? ObjectName { get; set; }

	public string? ObjectNameKh { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion
}