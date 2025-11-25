using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.PruCORE;

[Table("[dbo].[CmdbToAppMapping]"), DisplayName("CMDB to App Mapping")]
public class CmdbToAppMapping : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(CmdbToAppMapping).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "cmdb_to_app_mapping";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[RegularExpression(@"^[a-zA-Z0-9]{6}$", ErrorMessage = "Infra Stack must be 6 lower letters or digits.")]
	public string? InfraStackID { get; set; }
	[RegularExpression(@"^[A-Z0-9]{3}$", ErrorMessage = "Project Code must be 3 upper letters or digits.")]
	public string? ProjectCode { get; set; }
	public string? ProjectName { get; set; }
	public string? MeterCategory { get; set; }
	public string? App { get; set; }
	public string? AppID { get; set; }
	public string? ServiceName { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion
}