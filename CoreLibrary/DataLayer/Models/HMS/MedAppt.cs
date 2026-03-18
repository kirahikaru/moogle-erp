using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;
namespace DataLayer.Models.HMS;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[hms].[MedAppt]"), DisplayName("Medical Appointment")]
public class MedAppt : WorkflowEnabledObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(MedAppt).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "med_appt";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public DateTime? Date { get; set; }
	public int? DurationHour { get; set; }
	public int? DurationMin { get; set; }
	public DateTime? StartTime { get; set; }
	public DateTime? EndTime { get; set; }
	public int? CustomerId { get; set; }
	public int? PatientId { get; set; }
	public int? DoctorId { get; set; }
	public int? HealthcareFacilityId { get; set; }
	public string? DiagnosisNote { get; set; }
    public string? RecommendationNote { get; set; }
    public string? PrescriptionNote { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Patient? Patient { get; set; }

	[Computed, Write(false)]
	public Customer? Customer { get; set; }
	[Computed, Write(false)]
	public Doctor? Doctor { get; set; }

	[Computed, Write(false)]
	public HealthcareFacility? HealthcareFacility { get; set; }
	[Computed, Write(false)]
	public List<MedApptDx> Diagnosis { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion
	public MedAppt()
	{
		Diagnosis = [];
	}
}