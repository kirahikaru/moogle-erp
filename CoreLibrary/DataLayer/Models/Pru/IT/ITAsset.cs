using DataLayer.GlobalConstant.Pru;
using DataLayer.Models.SysCore.NonPersistent;
using PruHR = DataLayer.Models.Pru.HR;

namespace DataLayer.Models.Pru.IT;

[Table("[dbo].[ITAsset]"), DisplayName("IT Asset")]
public class ITAsset : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ITAsset).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "it_asset";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Asset ID' is required.")]
    //[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Asset ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
	[DisplayName("Asset ID")]
    public new string? ObjectCode { get; set; }

	/// <summary>
	/// Asset Name
	/// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Asset name is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

	/// <summary>
	/// Asset Type
	/// </summary>
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Asset Type' is required.")]
	public string? AssetType { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Product Name' is required.")]
	public string? ProductName { get; set; }
	public string? AssetName { get; set; }

	/// <summary>
	/// Asset Code from Finance
	/// </summary>
	public string? FinAssetCode { get; set; }
	public string? Manufacturer { get; set; }
	public string? SerialNo { get; set; }
	public string? SKU { get; set; }
	public string? CategoryCode { get; set; }
	public string? CategoryName { get; set; }
	public string? SubCategory { get; set; }
	public string? SvcDeskTicketNo { get; set; }
	public string? RequestUserID { get; set; }
	public string? RequestUserName { get; set; }
	public string? CurrentUserID { get; set; }
	public string? CurrentUserName { get; set; }
	public string? CurrentUserFunc { get; set; }
	public string? CurrentUserDept { get; set; }
	public string? CurrentUsagePurpose { get; set; }
	public string? SpecSummary { get; set; }
	public string? Remark { get; set; }

	/// <summary>
	/// 
	/// </summary>
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Asset State' is required.")]
	public string? AssetState { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Life Cycle Status' is required.")]
	public string? LifeCycleStatus { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? ExpiryDate { get; set; }
	public DateTime? EndDate { get; set; }

	/// <summary>
	/// End-Of-Support Date
	/// </summary>
	public DateTime? EOSDate { get; set; }
	/// <summary>
	/// End-Of-Life Date
	/// </summary>
	public DateTime? EOLDate { get; set; }
	public string? VendorID { get; set; }
	public string? VendorName { get; set; }
	public string? PRRefNo { get; set; }
	public string? PORefNo { get; set; }
	public string? QuoteRefNo { get; set; }
	public string? InvoiceRefNo { get; set; }
	public DateTime? PurchaseDate { get; set; }
	public decimal? PurchaseCost { get; set; }	
	public decimal? PurchaseTax { get; set; }
	public string? CurrentSite { get; set; }
	public string? CurrentLocation { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public List<ITAssetAuditTrail> AuditTrails { get; set; }

	[Computed, Write(false)]
	public ITAssetServerInfo? ServerInfo { get; set; }

	[Computed]
	public PruHR.Employee? Requestor { get; set; }

	[Computed]
	public PruHR.Employee? CurrentUser { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string ObjectNameAndCode => $"{ObjectName.NonNullValue("-")} ({ObjectCode.NonNullValue("-")})";

	[Computed, Write(false), ReadOnly(true)]
	public string AssetStateText => AssetStates.GetDisplayText(AssetState);

	[Computed, Write(false), ReadOnly(true)]
	public string LifeCycleStatusText => AssetLifeCycleStatuses.GetDisplayText(LifeCycleStatus);
	#endregion

	public ITAsset() : base()
    {
		AuditTrails = [];
    }
}