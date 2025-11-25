using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;


[Table("EmployeeEducation")]
public class EmployeeEducation : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(EmployeeEducation).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"employee_education";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? EmployeeId { get; set; }
    public int? EducationQaulificationId { get; set; }
    public string? EducationFieldOfStudyId { get; set; }
    public DateTime? MonthFrom { get; set; }
    public DateTime? MonthTo { get; set; }
    public bool IsVerfied { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}