using DataLayer.GlobalConstant;

namespace DataLayer.Models.HMS;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[hms].[MedicalExam]"), DisplayName("Medical Exam")]
public class MedicalExam : WorkflowEnabledObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(MedicalExam).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "medical_exam";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public DateTime? RequestDate { get; set; }
    public int? RequestorUserId { get; set; }
    public int? DoctorId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerIDCode { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Doctor? Doctor { get; set; }

	[Computed, Write(false)]
	public Customer? Customer { get; set; }

	[Computed, Write(false)]
	public User? RequestorUser { get; set; }

	[Computed, Write(false)]
	public User? AssignedUser { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion
}