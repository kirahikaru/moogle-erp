using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Hospital;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[hms].[MedicalApptDx]"), DisplayName("Diagnosis")]
public class MedicalAppointmentDiagnosis : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "MedicalApptDx";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "medical_appt_dx";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? MedicalAppointmentId { get; set; }

    #endregion

    #region *** LINKED OBJECTS ***
    
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion
}