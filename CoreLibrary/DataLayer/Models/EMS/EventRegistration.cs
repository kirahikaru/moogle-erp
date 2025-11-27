using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;

namespace DataLayer.Models.EMS;

[Table("[ems].[EventRegistration]"), DisplayName("Event Registration")]
public class EventRegistration : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.EVENT;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(EventRegistration).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "event_registration";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public DateTime? RegisteredDateTime { get; set; }
    [MaxLength(150)]
    public string? FullDisplayNameEn { get; set; }

    [MaxLength(150)]
    public string? FullDisplayNameKh { get; set; }

    [MaxLength(150)]
    public string? CallName { get; set; }

    /// <summary>
    /// Valid Values > GlobalConstants > Genders
    /// </summary>
    [MaxLength(1)]
    public string? Gender { get; set; }

    /// <summary>
    /// Valid Values > GlobalConstants > MaritalStatuses
    /// </summary>
    [MaxLength(1)]
    public string? MaritalStatus { get; set; }

    [MaxLength(30)]
    public string? Barcode { get; set; }
    public string? GuestOf { get; set; }
    public string? Grouping { get; set; }

    public int? EventInvitationId { get; set; }

    [Required(ErrorMessage = "Event is required.")]
    public int? EventId { get; set; }
    public int? PersonId { get; set; }

    [Precision(18, 2)]
    [DataType(DataType.Currency)]
    [Range(0.00, 99999999999999.00, ErrorMessage = "Fee Amount Paid must be positive")]
    public decimal? FeeAmountPaidUsd { get; set; }

    [Precision(18, 2)]
    [DataType(DataType.Currency)]
    [Range(0.00, 99999999999999.00, ErrorMessage = "Fee Amount Paid must be positive")]
    public decimal? FeeAmountPaidKhr { get; set; }

    [MaxLength(3)]
    public string? FeeCurrencyCode { get; set; }

    [DataType(DataType.Currency)]
    [Range(0.00, 99999999999999.00, ErrorMessage = "Fee Amount Paid must be positive")]
    public decimal? FeeAmountPaid { get; set; }
    public string? Remark { get; set; }

	/// <summary>
	/// GlobalConstants > EMS > EventRegistrationAttenanceCodes
	/// </summary>
	[Required(ErrorMessage ="'Attendance' is required.")]
    public string? AttendanceCode { get; set; }

    public bool IsCancelled { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Event? Event { get; set; }

	[Computed, Write(false)]
	public EventInvitation? Invitation { get; set; }

	[Computed, Write(false)]
	public Person? Person { get; set; }

	[Computed, Write(false)]
	public Currency? FeeCurrency { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string GenderText => Genders.GetDisplayText(Gender);

	[Computed, Write(false), ReadOnly(true)]
	public string MaritalStatusText => MaritalStatuses.GetDisplayText(MaritalStatus);

	[Computed, Write(false), ReadOnly(true)]
	public string StatusText => EventInvitationId.HasValue ? "Invited" : "Walk-In";

	[Computed, Write(false), ReadOnly(true)]
	public string AttendanceText => EventRegAttndCodes.GetDisplayText(AttendanceCode);
	/// <summary>
	/// Display in format: USD #,###.00
	/// </summary>
	[Computed, Write(false), ReadOnly(true)]
	public string FeeAmountPaidUSDText1 => FeeAmountPaidUsd == null ? "" : "USD" + FeeAmountPaidUsd.Value.ToString("#,###.00");

	/// <summary>
	/// Display in format: #,###.00
	/// </summary>
	[Computed, Write(false), ReadOnly(true)]
	public string FeeAmountPaidUSDText2 => FeeAmountPaidUsd == null ? "" : FeeAmountPaidUsd.Value.ToString("#,###.00");

	/// <summary>
	/// Display in format: KHR #,###.00
	/// </summary>
	[Computed, Write(false), ReadOnly(true)]
	public string FeeAmountPaidKHRText1 => FeeAmountPaidKhr == null ? "" : "KHR" + FeeAmountPaidKhr.Value.ToString("#,###");

	/// <summary>
	/// Display in format: #,###.00
	/// </summary>
	[Computed, Write(false), ReadOnly(true)]
	public string FeeAmountPaidKHRText2 => FeeAmountPaidKhr == null ? "" : FeeAmountPaidKhr.Value.ToString("#,###");

	/// <summary>
	/// Display in format: XXX #,###.00
	/// </summary>
	[Computed, Write(false), ReadOnly(true)]
	public string FeeAmountPaidText
    {
        get {
            if (string.IsNullOrEmpty(FeeCurrencyCode) || FeeAmountPaid == null)
                return "-";
            else if (FeeCurrencyCode.Is(Currencies.CAMBODIA_KHR, Currencies.VIETNAM_VND, Currencies.THAI_THB))
            {
                return $"{FeeCurrencyCode} {FeeAmountPaid!.Value:#,##0}";
            }
            else
            {
                return $"{FeeCurrencyCode} {FeeAmountPaid!.Value:#,##0.00}";
            }
        }
    }
    #endregion

    public EventRegistration()
    {
        IsCancelled = false;
        AttendanceCode = EventRegAttndCodes.PRESENT;
    }
}