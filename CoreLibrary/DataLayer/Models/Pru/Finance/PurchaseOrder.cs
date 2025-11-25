using Pru_GC = DataLayer.GlobalConstant.Pru;
using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.Finance;

[Table("[dbo].[PurchaseOrder]"), DisplayName("Purchase Order")]
public class PurchaseOrder : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(PurchaseOrder).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "purchase_order";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	//[Required(AllowEmptyStrings = false, ErrorMessage = "'Asset ID' is required.")]
	//[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Asset ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	//[MaxLength(80)]
	//public new string? ObjectCode { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "Asset name is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

	/// <summary>
	/// 
	/// </summary>
	[Required(ErrorMessage = "'Date' is required.")]
	public DateTime? EffectiveDate { get; set; }
	public DateTime? ExpiryDate { get; set; }
	/// <summary>
	/// Purchase Request ID
	/// </summary>
	[Required(AllowEmptyStrings = false, ErrorMessage = "'PR#' is required.")]
	public string? PRID { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'LBU' is required.")]
	public string? LBU { get; set; }

	public string? VendorID { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Workflow Status' is required.")]
	public string? WorkflowStatus { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Currency' is required.")]
	public string? CurrencyCode { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Total Amount' is required.")]
	[Range(0.00, 999999999999999.99, ErrorMessage = "'Total Amount' must be postive number.")]
	public decimal? TotalAmount { get; set; }
	public decimal? TotalTaxAmount { get; set; }
	public int? QuotationId { get; set; }
	public string? QuotationCode { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public List<PurchaseOrderItem> Items { get; set; }

	[Computed, Write(false)]
	public Vendor? Vendor { get; set; }

	[Computed, Write(false)]
	public Quotation? Quotation { get; set; }

	
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
			StringBuilder sb = new("[PO]");

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

			if (!string.IsNullOrEmpty(ObjectCode))
				sb.Append(" #" + ObjectCode!);
			else
				sb.Append(" #????????");


			return sb.ToString();
		}
	}
	#endregion

	public PurchaseOrder() : base()
    {
		Items = [];
		CurrencyCode = Pru_GC.Currencies.USD;
		LBU = Pru_GC.PruLBUs.PCLA;
	}
}