using DataLayer.GlobalConstant;
using DataLayer.Models.TSM;
using Dapper.Contrib.Extensions;

namespace DataLayer.Models.Tech;

/// <summary>
/// RAM | Random Access Memory
/// </summary>
[Table("[vts].[RAM]")]
public class RAM : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(RAM).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"ram";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***

	public string? SpecCode { get; set; }
	public string? Capacity { get; set; }
	public string? Brand { get; set; }
	public string? SerieName { get; set; }
	public string? Model { get; set; }
	public string? MemoryType { get; set; }
	public string? OCProfileSupport { get; set; }

	/// <summary>
	/// Unit: MT/s
	/// </summary>
	public int? TestedSpeedXMP { get; set; }
	public string? TestedLatencyXMP { get; set; }

	/// <summary>
	/// Unit: V
	/// </summary>
	public decimal? TestedVoltageXMP { get; set; }
	public int? SPDSpeed { get; set; }
	/// <summary>
	/// Unit: V
	/// </summary>
	public decimal? SPDVoltage { get; set; }
	public bool IsFanIncluded { get; set; }
	public string? Features { get; set; }
	public string? ProductInfoUrl { get; set; }

	public int? ItemId { get; set; }
	public decimal? LocalUnitPrice { get; set; }
	public decimal? MSRP { get; set; }

	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public List<TechSpecItem> SpecItems { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string LocalUnitPriceText => LocalUnitPrice.HasValue ? $"$ {LocalUnitPrice.Value:#,##0.00}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string MSRPText => MSRP.HasValue ? $"$ {MSRP.Value:#,##0.00}" : "-";
	#endregion

	public RAM()
	{
		SpecItems = [];
	}
}
