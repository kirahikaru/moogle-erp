using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Hobby;

[Table("[home].[BoardgameContentItem]")]
public class BoardgameContentItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOME_INVENTORY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(BoardgameContentItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "boardgame_content_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? BoardgameId { get; set; }
    public int? SequenceNo { get; set; }
    public int ItemCount { get; set; }
    public string? Remark { get; set; }
    #endregion

    #region *** LINKED OBJECT ***
    /// <summary>
    /// 
    /// </summary>
    [Computed, Write(false)]
	public Boardgame? Boardgame { get; set; }
    #endregion

    #region *** DYANMIC PROPERTIES ***
    #endregion
}