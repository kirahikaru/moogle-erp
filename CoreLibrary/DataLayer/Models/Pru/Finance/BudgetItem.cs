using Pru_GC = DataLayer.GlobalConstant.Pru;
using DataLayer.Models.SysCore.NonPersistent;
namespace DataLayer.Models.Pru.Finance;

[Table("[dbo].[BudgetItem]"), DisplayName("Budget Item")]
public class BudgetItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(BudgetItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "budget_item";

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

	public string? GroupingL1 { get; set; }
	public string? GroupingL2 { get; set; }
	public string? GroupingL3 { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'LBU' is required.")]
	public string? LBU { get; set; }

	[RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid format. Valid format is 10 digit")]
	public string? AccountCode { get; set; }
	public string? AccountName { get; set; }

	[RegularExpression(@"^\d{4}[A-Z]{0,1}$", ErrorMessage = "Invalid format. Valid format is 4 digit with operation (A to E) at the end")]
	public string? ActivityTrackID { get; set; }

	public string? CurrencyCode { get; set; }
	public decimal? TotalExpenseAmount { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Budgetted Amount' is required.")]
	[Range(0.00, 999999999999999999.99, ErrorMessage = "'Budgeetted Amount' must be positive amount.")]
	public decimal? BudgetedAmount { get; set; }

	[Range(0.00, 999999999999999999.99, ErrorMessage = "'Base Amount' must be positive amount.")]
	public decimal? BaseAmount { get; set; }

	[Range(0.00, 999999999999999999.99, ErrorMessage = "'Tax Rate' must be positive amount.")]
	public decimal? TaxRate { get; set; }

	[Range(0.00, 999999999999999999.99, ErrorMessage = "'Tax Amount' must be positive amount.")]
	public decimal? TaxAmount { get; set; }
	public decimal? BufferAmount { get; set; }
	public string? Category { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Expense Type' is required")]
	public string? ExpenseType { get; set; }
	public string? Class1 { get; set; }
	public string? Class2 { get; set; }
	public string? CostDriver { get; set; }
	[Required(ErrorMessage = "'Budget Year' is required")]
	[Range(0, 9999, ErrorMessage = "'Budget Year' must be positive amount.")]
	public int? BudgetYear { get; set; }
	public DateTime? SubmDate { get; set; }
	public DateTime? ApprovedDate { get; set; }
	public string? Remark { get; set; }
	public string? VersionName { get; set; }
	public bool IsCurrent { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed]
	public List<ExpenseItem> ExpenseItems { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string BudgettedAmountText => BudgetedAmount.HasValue ? BudgetedAmount!.Value.ToString("$ #,##0") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string TaxRateText => TaxRate.HasValue ? $"{TaxRate.Value:#,##0} %" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public decimal RemainingAmount => BudgetedAmount!.Value - (TotalExpenseAmount ?? 0);
	#endregion

	public BudgetItem() : base()
    {
		ExpenseItems = [];
		CurrencyCode = Pru_GC.Currencies.USD;
		IsCurrent = true;
    }
}