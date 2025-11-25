using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.HomeInventory;
[Table("[home].[OwnedItemAttachment]")]
public class OwnedItemAttachment : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOME_INVENTORY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(OwnedItemAttachment).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "owned_item_attachment";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? OwnedItemId { get; set; }
	public int DisplayOrder { get; set; }
	public string? FileType { get; set; }
	public string? Filename { get; set; }
	public string? FileExtension { get; set; }
	public byte[]? Content { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    
    #endregion

    #region *** DYANMIC PROPERTIES ***

    #endregion
}