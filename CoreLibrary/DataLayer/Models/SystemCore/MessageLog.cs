using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("MessageLog")]
public class MessageLog : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public static string TableName => $"{typeof(MessageLog).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"message_log";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? MessageType { get; set; }
    public int? LinkedObjectId { get; set; }
    public string? CustomerID { get; set; }
    public string? CustomerType { get; set; }
    public string? PolicyNumber { get; set; }
    public string? SenderPlatformID { get; set; }
    public string? ReceiverPlatformID { get; set; }
    public string? TelcoOperator { get; set; }
    public string? MessageText { get; set; }
    public DateTime? SendDateTime { get; set; }
    public DateTime? DeliveredDateTime { get; set; }
    public DateTime? ReceivedDateTime { get; set; }
    public string? MessageStatus { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion
}