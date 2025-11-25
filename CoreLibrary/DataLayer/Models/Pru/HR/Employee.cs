using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.HR;

[Table("[dbo].[Employee]"), DisplayName("Employee")]
public class Employee : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Employee).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "employee";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	//[Required(AllowEmptyStrings = false, ErrorMessage = "'ID' is required.")]
	[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Employee Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
	public string? LBU { get; set; }
	public string? EmpID { get; set; }
	public string? Surname { get; set; }
	public string? GivenName { get; set; }
	public string? LocalName { get; set; }
	public string? CallName { get; set; }
	public DateTime? BirthDate { get; set; }
	public DateTime? HiredDate { get; set; }
	public DateTime? JoinedDate { get; set; }
	public DateTime? LastDay { get; set; }
	public string? JobGrade { get; set; }
	public string? JobTitle { get; set; }
	public string? JobFamily { get; set; }
	public string? JobFunction { get; set; }
	public string? Unit { get; set; }
	public string? Section { get; set; }
	public string? Division { get; set; }
	public string? Function { get; set; }
	public string? Department { get; set; }
	public string? SaleTag { get; set; }
	public string? WorkerType { get; set; }
	public string? EmpStatus { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false)]
	public string ObjectNameAndCode => $"{ObjectName.NonNullValue("-")} ({ObjectCode.NonNullValue("-")})";
    #endregion

    public Employee() : base()
    {
        
    }
}