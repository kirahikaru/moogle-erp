using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Hobby;

[Table("[home].[ComicCharacter]")]
public class ComicCharacter : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOME_INVENTORY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ComicCharacter).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "comic_character";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public string? Type { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region DYANMIC PROPERTIES

    #endregion
}
