using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.EventManagement;

[Table("[ems].[EventType]"), DisplayName("Event Type")]
public class EventType : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.EVENT;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(EventType).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "event_type";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'ID' is required.")]
	[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	[MaxLength(80)]
	public new string? ObjectCode { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Name' is required.")]
	[MaxLength(255)]
	public new string? ObjectName { get; set; }
	public string? ObjectNameKh { get; set; }
    public bool IsEnabled { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public List<EventOrganizerRole> ValidRoles { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    
    #endregion

    public EventType()
    {
        ValidRoles = [];
        IsEnabled = true;
    }
}