using DataLayer.GlobalConstant;

namespace DataLayer.Models.LIB;

[Table("[lib].[BookPersonRoleMap]")]
public class BookPersonRoleMap
{
	[Computed, Write(false), ReadOnly(true)]
	public static string SchemaName => SysDbSchemaNames.LIBRARY;

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTableName => typeof(BookPersonRoleMap).Name;

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTableName => "book_person_role_map";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? BookId { get; set; }
    public int? PersonId { get; set; }

    /// <summary>
    /// ValidValue > GlobalConstants.BookRoles
    /// </summary>
    public string? BookRole { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}