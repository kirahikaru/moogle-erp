namespace DataLayer.Models.SysCore;

[Table("UserNotif")]
public class UserNotification : AuditObject
{
	[Computed, Write(false), ReadOnly(false)]
	public new static string MsSqlTableName => $"UserNotif";

	[Computed, Write(false), ReadOnly(false)]
	public new static string PgTableName => "user_notif";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	//public new Int64 Id { get; set; }
	public int? UserId { get; set; }
    public string? Username { get; set; }
    public int? NotificationId { get; set; }
    public DateTime? PushedDateTime { get; set; }
    public bool IsNew { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion

}