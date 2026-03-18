namespace DataLayer.Models.Pru.Finance;

using Dapper.Contrib.Extensions;
/// <summary>
/// Finance Actual Summary Entry
/// </summary>
[Table("[dbo].[FinActSummEntry]"), DisplayName("Finance Actual Summary Entry")]
public class FinActSummEntry : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(FinActSummEntry).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "fin_act_summ_entry";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Budget ID' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Budget ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Budget Line Description' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
	public string? LBU { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Budget Item' is required.")]
	public int? BudgetItemId { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Report Year' is required.")]
	public int? Yr { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Report Month' is required.")]
	public int? Mth { get; set; }
	public string? Remark { get; set; }
	public decimal? ForecastAmount { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Total Amount' is required.")]
	public decimal? TotalAmount { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string YyyyMmText => (Yr != null ? Yr.Value.ToString() : "") + "-" + (Mth != null ? Mth.Value.ToString("D2") : "");
	#endregion

	public FinActSummEntry() : base()
    {
		
    }
}