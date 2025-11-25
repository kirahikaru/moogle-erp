using DataLayer.AuxComponents.DataAnnotations;

namespace DataLayer.Models.SysCore;

[Table("Notification")]
public class Notification : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public static string TableName => $"{typeof(Notification).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"notification";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[StringLength(1000), StringUnicode(true)]
    public string? Content { get; set; }

    /// <summary>
    /// Valid Values: GlobalConstants.NotificationTypes
    /// </summary>
    [StringLength(30)]
    public string? NotificationTypeCode { get; set; }
    public bool IsGroupNotification { get; set; }
    public int? UserId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion
}