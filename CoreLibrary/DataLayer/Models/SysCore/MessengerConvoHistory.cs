using DataLayer.AuxComponents.DataAnnotations;

namespace DataLayer.Models.SysCore;

[Table("MessengerConversationHistory"), DisplayName("Messenger Conversation History")]
public class MessengerConvoHistory : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public static string TableName => $"{typeof(MessengerConvoHistory).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"messenger_convo_history";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	//public new Int64 Id { get; set; }
	/// <summary>
	/// Valid values: GlobalConstant.ContactChannels
	/// </summary>
	public string? PlatformCode { get; set; }
    public string? SenderUserId { get; set; }
    public string? ReceiverUserId { get; set; }
    public string? ConversationId { get; set; }
    [MaxLength(9999), StringUnicode(true)]
    public string? ConversationText { get; set; }
    public DateTime? DeliveredDateTime { get; set; }
    public DateTime? ReadDateTime { get; set; }
    public string? MessageStatus { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion
}