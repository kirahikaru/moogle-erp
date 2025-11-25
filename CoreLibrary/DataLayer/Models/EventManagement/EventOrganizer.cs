using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.EventManagement;

[Table("[ems].[EventOrganizer]")]
public class EventOrganizer : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.EVENT;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(EventOrganizer).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "event_organizer";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? EventId { get; set; }
    public int? PersonId { get; set; }
    public int? EventOrganizerRoleId { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Person? Person { get; set; }

    [Computed, Write(false)]
    public EventOrganizerRole? Role { get; set; }
    #endregion

    #region *** DYANMIC PROPERTIES ***

    #endregion
}