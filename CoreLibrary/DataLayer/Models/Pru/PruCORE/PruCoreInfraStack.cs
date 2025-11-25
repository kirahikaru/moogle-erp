using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.PruCORE;

[DisplayName("Infra Stack")]
[Table("[dbo].[PruCoreInfraStack]")]
public class PruCoreInfraStack : AuditObject
{

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(PruCoreInfraStack).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "gl_account";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	public string? AppID { get; set; }
	public string? AppName { get; set; }
	public string? ActualDesc { get; set; }
	public string? ProjectCode { get; set; }
	public string? LBU { get; set; }
	public string? Owners { get; set; }

	/// <summary>
	/// pcla | plal | mmlife
	/// </summary>
	public string? BillingGrp { get; set; }

	/// <summary>
	/// khlife, mmlife, lalife
	/// </summary>
	public string? AzureLBU { get; set; }
	/// <summary>
	/// prod | nprd
	/// </summary>
	public string? Env { get; set; }
	/// <summary>
	/// dev | uat | sit | prd | nsc | nhb
	/// </summary>
	public string? Stage { get; set; }

	/// <summary>
	/// Active | Decomissioned
	/// </summary>
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Status' is required.")]
	public string? Status { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false), ReadOnly(true)]
	public PruCoreProject? Project { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}