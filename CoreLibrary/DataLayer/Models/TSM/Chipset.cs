using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;
namespace DataLayer.Models.TSM;

/// <summary>
/// CPU | Central Processing Unit
/// </summary>
/// <remarks>
/// https://www.intel.com/content/www/us/en/ark/products/series/236803/intel-core-ultra-processors-series-1.html#@nofilter
/// https://www.amd.com/en/products/specifications/processors.html
/// </remarks>
[Table("[vts].[Chipset]")]
public class Chipset : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(Chipset).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"chipset";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***

	public string? Brand { get; set; }
	public string? Series { get; set; }
	public string? Summary { get; set; }
	public string? Model { get; set; }
	public string? ProductUrl { get; set; }

	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public List<TechSpecItem> SpecItems { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***

	#endregion

	public Chipset()
	{
		SpecItems = [];
	}
}
