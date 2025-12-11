using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.Finance;

[Table("FinActTracker"), DisplayName("Finance Activity Tracker")]
public class FinActivityTracker : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "FinActTracker";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "fin_act_tracker";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Account Code' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Account Code' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Account Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
	public string? OwnerEmpID { get; set; }
	public string? OwnerName { get; set; }
	public string? FunctionName { get; set; }
    public string? DepartmentName { get; set; }
    public string? Remark { get; set; }

    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
	#endregion

	public FinActivityTracker() : base()
    {
		
    }
}