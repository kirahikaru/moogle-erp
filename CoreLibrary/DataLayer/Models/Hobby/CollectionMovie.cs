using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Hobby;

[Table("[home].[CollectionMovie]")]
public class CollectionMovie : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOME_INVENTORY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(CollectionMovie).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "collection_movie";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public int? Year { get; set; }
    /// <summary>
    /// DataSetup => DropdownDataList
    /// SystemName = Hobby
    /// </summary>
    public string? Category { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? Resolution { get; set; }
    /// <summary>
    /// DataSetup => DropdownDataList
    /// SystemName = Hobby
    /// </summary>
    public string? OwnershipStatus { get; set; }
    public int? MyRating { get; set; }
    public string? SeriesName { get; set; }
    public string? OriginalFileName { get; set; }
    public int? OwnerUserId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region DYANMIC PROPERTIES

    #endregion
}
