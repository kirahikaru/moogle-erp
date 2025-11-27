using DataLayer.GlobalConstant;

namespace DataLayer.Models.LIB;

[Table("[lib].[BookBookGenre]")]
public class BookBookGenre
{
	[Computed, Write(false), ReadOnly(true)]
	public static string SchemaName => SysDbSchemaNames.LIBRARY;

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTableName => typeof(BookBookGenre).Name;

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTableName => "book_book_genre";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? BookId { get; set; }
    public int? BookGenreId { get; set; }
    public string? GenreName { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}