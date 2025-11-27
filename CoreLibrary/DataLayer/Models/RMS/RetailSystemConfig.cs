using DataLayer.GlobalConstant;

namespace DataLayer.Models.RMS;

[Table("[rms].[RetailSystemConfig]")]
public class RetailSystemConfig : AuditObject
{
    public string? SystemCurrencyCode { get; set; }

	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(RetailSystemConfig).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "retail_system_config";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);
}