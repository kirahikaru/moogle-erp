using DataLayer.GlobalConstant;

namespace DataLayer.Models.EMS;

[Table("[ems].[EventOrganizerRole]")]
public class EventOrganizerRole : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.EVENT;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(EventOrganizerRole).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "event_organizer_role";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? EventTypeId { get; set; }
    public string? ObjectNameKh { get; set; }
    public bool IsEnabled { get; set; }
    #endregion

    public EventOrganizerRole() : base()
    {
        IsEnabled = true;
    }
}