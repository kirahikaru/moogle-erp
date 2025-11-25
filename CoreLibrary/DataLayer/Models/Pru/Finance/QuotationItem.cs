using DataLayer.Models.Pru.IT;
using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.Finance;

[Table("[dbo].[QuotationItem]"), DisplayName("QuotationItem")]
public class QuotationItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(QuotationItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "quotation_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Asset ID' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Asset ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Asset name is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
	public int? OrderNo { get; set; }
	/// <summary>
	/// 
	/// </summary>
	public string? SKU { get; set; }
	public string? Type { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Quantity { get; set; }
	public decimal? UnitPrice { get; set; }
	public decimal? TotalAmount { get; set; }
	public decimal? TotalTaxAmount { get; set; }
	public string? Remark { get; set; }
	public int? QuotationId { get; set; }
	public string? QuotationCode { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public List<ITAssetAuditTrail> AuditTrails { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	
	#endregion

	public QuotationItem() : base()
    {
		AuditTrails = [];
    }
}