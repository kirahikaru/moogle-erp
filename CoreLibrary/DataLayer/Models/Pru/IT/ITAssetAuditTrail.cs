using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.IT;

[Table("[dbo].[ITAssetAuditTrail]"), DisplayName("IT Asset Audit Trail")]
public class ITAssetAuditTrail : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ITAssetAuditTrail).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "it_asset_audit_trail";

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

	public int? AssetId { get; set; }
	public string? AssetCode { get; set; }
	public string? SerialNo { get; set; }
	public string? Category { get; set; }
	public string? SubCategory { get; set; }
	[Required(ErrorMessage = "'Request Date' is required.")]
	public DateTime? RequestDate { get; set; }

	public DateTime? EffectiveDate { get; set; }
	public DateTime? EffTillDate { get; set; }
	[Required(ErrorMessage = "'Action' is required.")]
	public string? ActionDesc { get; set; }
	public string? ActionUser { get; set; }
	public string? CurrentUserID { get; set; }
	public string? CurrentUserName { get; set; }
	public string? CurrentUserFunc { get; set; }
	public string? CurrentUserDept { get; set; }
	public string? Justification { get; set; }
	public string? Approver { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYNAMIC PROPERTIES ***
	
	#endregion

	public ITAssetAuditTrail() : base()
    {
        
    }
}