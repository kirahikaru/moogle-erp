using Pru_GC = DataLayer.GlobalConstant.Pru;
using DataLayer.Models.SysCore.NonPersistent;
namespace DataLayer.Models.Pru.Finance;

[Table("[dbo].[BudgetLine]"), DisplayName("Budget Line")]
public class BudgetLine : AuditObject, IParentChildHierarchyObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(BudgetLine).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "budget_line";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'ID' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Budget ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

	[Range(0, 99999999, ErrorMessage = "'Order #' must be between positive whole number.")]
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Order #' is required.")]
	public int OrderNo { get; set; }
	public string? DisplayOrder { get; set; }
	public bool IsEnabled { get; set; }

	public int? ParentId { get; set; }
	public string? ParentCode { get; set; }

	public string? HierarchyPath { get; set; }

	[Range(1, 99, ErrorMessage = "'Grouping Level' must be between 1 and 99.")]
	[Required(AllowEmptyStrings =false, ErrorMessage = "'Grouping Level' is required.")]
	public int? GroupingLevel { get; set; }
	public bool IsBudgetGroup { get; set; }
	public string? Description { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public BudgetLine? Parent { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	
	#endregion

	public BudgetLine() : base()
    {
		IsEnabled = true;
    }
}