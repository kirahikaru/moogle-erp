using DataLayer.GlobalConstant.Pru;
using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.Finance;

[Table("[dbo].[Invoice]"), DisplayName("Invoice")]
public class Invoice : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Invoice).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "invoice";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Invoice #' is required.")]
    //[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Asset ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Description' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'LBU' is required.")]
	public string? LBU { get; set; }
	/// <summary>
	/// 
	/// </summary>
	[Required(ErrorMessage = "'Invoice Date' is required.")]
	public DateTime? EffectiveDate { get; set; }
	public DateTime? ExpiryDate { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "Vendor is required.")]
	public string? VendorID { get; set; }
	public string? CustomerRef { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Workflow Status' is required.")]
	public string? WorkflowStatus { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Currency' is required.")]
	public string? CurrencyCode { get; set; }
	[Required(ErrorMessage = "'Total Amount' is required.")]
	public decimal? TotalAmount { get; set; }
	public decimal? TotalTaxAmount { get; set; }

	/// <summary>
	/// Accounting System Submission Record Ref. ID
	/// </summary>
	public string? AccSysSubmID { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed]
	public List<InvoiceItem> Items { get; set; }

	[Computed]
	public List<ExpenseItem> ExpenseItems { get; set; }

	[Computed]
	public Vendor? Vendor { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false)]
	public string VendorName => Vendor != null ? Vendor.ObjectName.NonNullValue("-") : VendorID.NonNullValue("-");


	[Computed, Write(false)]
	public string TotalAmountText => $"{CurrencyCode} {TotalAmount!.Value:#,##0}";

	[Computed, Write(false)]
	public string FileNameDisplay
	{
		get
		{
			StringBuilder sb = new("[INV]");

			if (!string.IsNullOrEmpty(VendorID))
				sb.Append($" {VendorID}");
			else
				sb.Append(" ???");

			if (EffectiveDate != null)
				sb.Append(" " + EffectiveDate!.Value.ToString("yyMMdd") + " - ");
			else
				sb.Append(" YYMMDD - ");

			if (!string.IsNullOrEmpty(ObjectName))
				sb.Append(ObjectName!);
			else
				sb.Append(" (missing description)");

			if(!string.IsNullOrEmpty(ObjectCode))
				sb.Append(" #" + ObjectCode!);
			else
				sb.Append(" #????????");


			return sb.ToString();
		}
	}
	#endregion

	public Invoice() : base()
    {
		Items = [];
		ExpenseItems = [];
		CurrencyCode = Currencies.USD;
		LBU = PruLBUs.PCLA;
    }
}