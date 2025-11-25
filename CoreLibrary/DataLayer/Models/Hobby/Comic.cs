using DataLayer.GlobalConstant;

namespace DataLayer.Models.Hobby;

[Table("[home].[Comic]")]
public class Comic : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOME_INVENTORY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Comic).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "comic";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public int? VolumeNo { get; set; }
    public string? Publisher { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region DYANMIC PROPERTIES
    
    #endregion
}
