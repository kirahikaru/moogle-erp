using DataLayer.GlobalConstant;

namespace DataLayer.Models.HMS;

/// <summary>
/// Medical Prescription
/// </summary>
[Table("[hms].[MedicalRx]"), DisplayName("Medical Prescription")]
public class MedRx : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "MedicalRx";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "medical_rx";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	[Required(AllowEmptyStrings = false, ErrorMessage = "'CODE' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Item 'NAME' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

    #region *** DATABASE FIELDS ***
    public int? HealthcareFacilityId { get; set; }
    public string? HealthcareFacilityCode { get; set; }
    /// <summary>
    /// Valid Values: GlobalConstants > Healthcare Facility Management
    /// </summary>
    public string? FacilityType { get; set; }
    public DateTime? IssueDateTime { get; set; }
    public int? CustomerId { get; set; }
    public int? DoctorId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
	public List<MedRxItem> PrescriptionItems { get; set; }

	[Computed, Write(false)]
	public HealthcareFacility? HealthcareFacility { get; set; }

	[Computed, Write(false)]
	public Customer? Customer { get; set; }

	[Computed, Write(false)]
	public Doctor? Doctor { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion

    public MedRx()
    {
        PrescriptionItems = new();
    }
}