using DataLayer.GlobalConstant;

namespace DataLayer.Models.HMS;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[hms].[Doctor]")]
public class Doctor : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Doctor).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "doctor";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ObjectNameKh { get; set; }

    [Required(ErrorMessage = "An active 'Employee Profile' is required to be selected.")]
    public int? EmployeeId { get; set; }

    [Required(ErrorMessage = "An active 'Employee Profile' is required to be selected.")]
    public string? EmployeeCode { get; set; }

    /// <summary>
    /// GlobalConstants > HMS > DoctorStatuses
    /// </summary>
    [Required(ErrorMessage = "'Status' is required.")]
    public string? Status { get; set; }

    [Required(ErrorMessage = "'Healthcare Facility' is required.")]
    public int? HealthcareFacilityId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Employee? Employee { get; set; }

	[Computed, Write(false)]
	public HealthcareFacility? HealthcareFacility { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion

    public Doctor()
    {
        Status = DoctorStatuses.DRAFT;
    }
}