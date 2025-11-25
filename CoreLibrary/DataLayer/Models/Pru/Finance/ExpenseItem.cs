using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.Finance;

[Table("ExpenseItem"), DisplayName("Expense Item")]
public class ExpenseItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ExpenseItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "expense_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'ID' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Expense Description' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
	public int? OrderNo { get; set; }
	public string? LBU { get; set; }
	public DateTime? EffectiveDate { get; set; }

	[RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid format. Valid format is 10 digit")]
	public string? AccountCode { get; set; }
	public string? AccountName { get; set; }

	[RegularExpression(@"^\d{4}[A-Z]{0,1}$", ErrorMessage = "Invalid format. Valid format is 4 digit with operation (A to E) at the end")]
	public string? ActivityTrackID { get; set; }
	public string? FinProjectCode { get; set; }
	public decimal? Amount { get; set; }
	public decimal? TaxRate { get; set; }
	public decimal? TaxAmount { get; set; }
	public int? ExpenseYr { get; set; }
	public int? ExpenseMth { get; set; }
	public DateTime? SubmDate { get; set; }
	public DateTime? ApprovedDate { get; set; }
	public string? PurchaseOrderNo { get; set; }
	public int? InvoiceId { get; set; }
	public int? InvoiceItemId { get; set; }
	public string? InvoiceCode { get; set; }
	public DateTime? InvoiceDate { get; set; }

	public string? EmpID { get; set; }
	/// <summary>
	/// Accounting System Reference Number
	/// </summary>
	public string? AccSysID { get; set; }

	/// <summary>
	/// [FK] Vendor.ObjectCode
	/// </summary>
	public string? VendorID { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public BudgetItem? BudgetItem { get; set; }

	[Computed, Write(false)]
	public Vendor? Vendor { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string VendorName => Vendor != null ? Vendor.ObjectName.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string BudgetItemName => BudgetItem != null ? BudgetItem.ObjectName.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string BudgetItemID => BudgetItem != null ? BudgetItem.ObjectCode.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string AmountWithTaxText => $"$ {(Amount ?? 0)+(TaxAmount ?? 0):#,##0.00}";

	[Computed, Write(false), ReadOnly(true)]
	public decimal TotalAmount => (Amount ?? 0) + (TaxAmount ?? 0);

	[Computed, Write(false), ReadOnly(true)]
	public string TaxRateText => TaxRate.HasValue ? $"{TaxRate.Value:#,##0} %" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string ExpenseYrMthTxt => (ExpenseYr.HasValue ? ExpenseYr.ToString() : "YYYY") + "-" + (ExpenseMth.HasValue ? ExpenseMth!.Value.ToString("00") : "-");
	#endregion

	public ExpenseItem() : base()
    {
		
    }
}