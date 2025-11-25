using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.Finance;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.EventManagement;

[Table("[ems].[Event]")]
public class Event : AuditObject
{
    [Computed, Write(false), ReadOnly(true)]
    public new static string SchemaName => SysDbSchemaNames.EVENT;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Event).Name;

    [Computed, Write(false), ReadOnly(true)]
    public new static string PgTableName => "event";

    [Computed, Write(false), ReadOnly(true)]
    public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

    [Computed, Write(false), ReadOnly(true)]
    public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Event ID' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Event Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

    [Required(ErrorMessage = "'Venue' is required.")]
    public string? VenueName { get; set; }

    [Required(ErrorMessage = "'Event Start Date/Time' is required.")]
    public DateTime? StartDateTime { get; set; }


    [Required(ErrorMessage = "'Event End Date/Time' is required.")]
    public DateTime? EndDateTime { get; set; }
    public bool IsSingleDayEvent { get; set; }

    /// <summary>
    /// Valid Values > DataLayer.GlobalConstant.EMS > EventFeeTypes
    /// </summary>
    [Required(ErrorMessage = "Event Fee Type is required.")]
    [MaxLength(25)]
    public string? FeeTypeCode { get; set; }

    [MaxLength(25)]
    public string? FeeCurrencyCode { get; set; }

    [Precision(18, 2)]
    public decimal? FeeAmount { get; set; }


    [Required(ErrorMessage = "'Event Type' is required.")]
    public int? EventTypeId { get; set; }
    public string? Description { get; set; }
    public string? Remark { get; set; }
    
    public string? WorkflowStatus { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public List<EventInvitation> Invitations { get; set; }

	[Computed, Write(false)]
	public List<EventRegistration> Registrations { get; set; }

	[Computed, Write(false)]
	public Currency? FeeCurrency { get; set; }

	[Computed, Write(false)]
	public EventType? EventType { get; set; }

	[Computed, Write(false)]
	public List<EventOrganizer> Organizers { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string FeeTypeDesc => EventFeeTypes.GetDisplayText(FeeTypeCode!);

	[Computed, Write(false), ReadOnly(true)]
	public string ObjectNameWithCode => $"{ObjectName} ({ObjectCode})";

    public string FeeTypeText
    {
        get
        {
            if (FeeTypeCode == EventFeeTypes.FIXED_FEE)
            {
                return "Fixed Fee of " + CurrencyExtension.ToDisplayText(FeeAmount!.Value, FeeCurrencyCode!);
            }
            else
                return EventFeeTypes.GetDisplayText(FeeTypeCode);
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string DisplayDescription
    {
        get {
            if (EndDateTime == null)
                return ObjectName!;

            if (EndDateTime != null && (EndDateTime == null || EndDateTime == EndDateTime))
				return string.Format("{0} ({1})", ObjectName, EndDateTime!.Value.ToString("ddd, dd MMM yyyy"));

            if (EndDateTime != null && EndDateTime != null)
                return string.Format("{0} ({1} to {2}", ObjectName, EndDateTime.Value.ToString("dd MMM yyyy"), EndDateTime.Value.ToString("dd MMM yyyy"));

            return string.Empty;
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string PeriodText
    {
        get
        {
            if (IsSingleDayEvent)
            {
                return StartDateTime!.Value.ToString("dddd, dd-MMM-yyyy");
            }
            else if (StartDateTime!.Value.Date == EndDateTime!.Value.Date)
            {
                return "From " + StartDateTime!.Value.ToString("dddd, dd-MMM-yyyy HH:mm tt")  + " to " + EndDateTime!.Value.ToString("dddd, dd-MMM-yyyy HH:mm tt");
            }
            else
            {
                return StartDateTime!.Value.ToString("dddd, dd-MMM-yyyy") + " from " + StartDateTime!.Value.ToString("HH:mm tt") + " to " + EndDateTime!.Value.ToString("HH:mm tt");
            }
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string StartDateTimeText
    {
        get
        {
            if (StartDateTime != null)
            {
                if (StartDateTime.Value.Hour == 0 && StartDateTime.Value.Minute == 0 && StartDateTime.Value.Second == 0)
                    return StartDateTime.Value.ToString("dd-MMM-yyyy");
                else
                    return StartDateTime.Value.ToString("dd-MMM-yyyy hh:mm tt");
            }
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string EndDateTimeText
    {
        get
        {
            if (EndDateTime != null)
            {
                if (EndDateTime.Value.Hour == 0 && EndDateTime.Value.Minute == 0 && EndDateTime.Value.Second == 0)
                    return EndDateTime.Value.ToString("dd-MMM-yyyy");
                else
                    return EndDateTime.Value.ToString("dd-MMM-yyyy hh:mm tt");
            }
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string EventTimeText
    {
        get
        {
            StringBuilder sb = new();

            if (IsSingleDayEvent)
            {
                if (StartDateTime.HasValue)
                {
                    sb.Append(StartDateTime.Value.ToString("dd-MMM-yyyy"));

                    if (StartDateTime.Value.Hour != 0 || StartDateTime.Value.Minute != 0 || StartDateTime.Value.Second != 0)
                        sb.Append($" | {StartDateTime.Value.ToString("hh:mm tt")}");

                    if (EndDateTime.HasValue && (StartDateTime.Value.Hour != 0 || StartDateTime.Value.Minute != 0 || StartDateTime.Value.Second != 0))
                        sb.Append($" - {EndDateTime.Value.ToString("hh: mm tt")}");
                }
                else
                    sb.Append(" - ");
            }
            else
                return "-";

            return sb.ToString();
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string FeeAmountText
    {
        get
        {
            if (FeeAmount.HasValue)
            {
                if (FeeAmount.Value % 1 > 0)
                    return (FeeCurrencyCode + " " + FeeAmount.Value.ToString("#,##0.00"));
                else
                    return (FeeCurrencyCode + " " + FeeAmount.Value.ToString("#,##0"));
            }

            return " - ";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string WorkflowStatusText => EventWorkflowStatuses.GetDisplayText(WorkflowStatus);
    
    #endregion

    public Event() : base()
    {
        Invitations = [];
        Registrations = [];
        Organizers = [];
        WorkflowStatus = EventWorkflowStatuses.START;
    }
}