using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;
namespace DataLayer.Models.HMS;

/// <summary>
/// Medical Appointment Diagnosis
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[hms].[MedApptDx]"), DisplayName("Diagnosis")]
public class MedApptDx : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(MedApptDx).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "med_appt_dx";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? MedApptId { get; set; }
	public string? IllnessCode { get; set; }
	public int? IllnessId { get; set; }

	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Illness? Illness { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion
}