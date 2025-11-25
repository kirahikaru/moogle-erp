using DataLayer.GlobalConstant.Pru;
using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.IT;

[Table("[dbo].[PruLicO365]"), DisplayName("Pru O365 Licence")]
public class PruLicO365 : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(PruLicO365).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "pru_lic_o365";

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

	/// <summary>
	/// 
	/// </summary>
	public string? DisplayName { get; set; }
	public string? Licenses { get; set; }
	public string? Department { get; set; }
	public string? UsageLocation { get; set; }
	public string? AssignedProductSkus { get; set; }
	public string? JobTitle { get; set; }
	public string? FirstName { get; set; }
	public string? Office { get; set; }
	public string? Domain { get; set; }
	public string? OUnit { get; set; }
	public string? OU { get; set; }
	public string? BU { get; set; }
	public string? SAMAccountName { get; set; }
	public string? AccountType { get; set; }
	public string? AcctTypeUser { get; set; }
	public DateTime? LogonTimestamp { get; set; }
	public DateTime? LogonDate { get; set; }
	public bool LoggedInPast60Days { get; set; }
	public bool LoggedInPast30Days { get; set; }
	public bool IsAccountEnabled { get; set; }
	public DateTime? AccountCreatedDate { get; set; }
	public DateTime? ExpiryDate { get; set; }
	public string? Manager { get; set; }
	public bool IsDirSyncEnabled { get; set; }
	public bool IsGuestUser { get; set; }
	public string? EmploymentType { get; set; }
	public string? Grouping { get; set; }
	public string? Status { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public List<PruLicO365Action> Actions { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	public string? StatusText => AssetLifeCycleStatuses.GetDisplayText(Status);
	#endregion

	public PruLicO365() : base()
    {
		Actions = [];
    }
}