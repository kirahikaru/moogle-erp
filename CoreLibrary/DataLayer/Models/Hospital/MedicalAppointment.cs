using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Hospital;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[hms].[MedicalAppt]"), DisplayName("Medical Appointment")]
public class MedicalAppointment : WorkflowEnabledObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "MedicalAppt";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "medical_appt";

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
    public string? DiagnosisNote { get; set; }
    public string? RecommendationNote { get; set; }
    public string? PrescriptionNote { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Employee? Employee { get; set; }

	[Computed, Write(false)]
	public CambodiaAddress? RegisteredAddress { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion
}