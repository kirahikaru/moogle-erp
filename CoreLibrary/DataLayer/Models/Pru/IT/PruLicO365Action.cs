using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.IT;

[Table("[dbo].[PruLicO365Action]"), DisplayName("PruLicO365Action")]
public class PruLicO365Action : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(PruLicO365Action).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "pru_lic_o365_action";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Record ID' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Record ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

	/// <summary>
	/// 
	/// </summary>
	public string? StatusCode { get; set; }
	public string? ApprovedUser { get; set; }
	public DateTime? ApprovedDate { get; set; }
	public string? AssignedUser { get; set; }
	public DateTime? AssignedDate { get; set; }
	public DateTime? CompletedDate { get; set; }
	public DateTime? VerifiedDate { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYNAMIC PROPERTIES ***

	#endregion

	public PruLicO365Action() : base()
    {
		
    }
}