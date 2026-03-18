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
[Table("[vts].[CPU]")]
public class CPU : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(CPU).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"cpu";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***

	public string? CodeName { get; set; }
	public string? FullTrademarkName { get; set; }
	[Required]
	public string? Family { get; set; }
	public DateTime? LaunchDate { get; set; }
	public string? LaunchDateText { get; set; }
	[Required]
	public string? SeriesName { get; set; }
	public string? FormFactor { get; set; }
	[Range(0, 99999, ErrorMessage = "'No. of Core' must be positive whole number.")]
	public int? CoreCount { get; set; }
	[Range(0, 99999, ErrorMessage = "'No. of Core' must be positive whole number.")]
	public int? ThreadCount { get; set; }

	[Range(0.0, 99999.99, ErrorMessage = "'Max. Clock Speed (GHz)' must be positive.")]
	public decimal? MaxClockSpeedGHz { get; set; }
	[Range(0.0, 99999.99, ErrorMessage = "'Base Clock Speed (GHz)' must be positive.")]
	public decimal? BaseClockSpeedGHz { get; set; }
	public int? L2CacheSizeMb { get; set; }
	public int? L3CacheSizeMb { get; set; }
	public int? DefaultTDPWatt { get; set; }
	public int L1CacheSizeKb { get; set; }
	//AMD Configurable for TDP (cTDP)
	//Processor Technology for CPU Cores
	public bool IsUnlockedForOC { get; set; }
	public string? CPUSocket { get; set; }

	/// <summary>
	/// Thermal Solution (PIB)
	/// </summary>
	public string? ThermalSolutionPIB { get; set; }
	public string? ThermalSolutionMPK { get; set; }
	public string? RecommendedCooler { get; set; }

	/// <summary>
	/// Max. Operating Temperature (TjMax)
	/// </summary>
	public int? MaxOpsTemp { get; set; }
	public string? OSSupport { get; set; }
	public string? PCIExpressVer { get; set; }

	/// <summary>
	/// System Memory Type
	/// </summary>
	public string? SysMemType { get; set; }
	public int? MemoryChannel { get; set; }
	public string? IntegratedGPU { get; set; }

	/// <summary>
	/// Intel: Processor Base Power
	/// </summary>
	public int? ProcessorBasePower { get; set; }
	/// <summary>
	/// Intel: Maximum Turbo Power
	/// </summary>
	public int? MaxTurboPower { get; set; }


	/// <summary>
	/// System Memory Specification
	/// </summary>
	public string? SysMemSpec { get; set; }
	public string? ProductInfoUrl { get; set; }

	public int?	ItemId { get; set; }
	public decimal? LocalUnitPrice { get; set; }
	public decimal? MSRP { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public List<DeviceIOPort> IOPorts { get; set; }

	[Computed, Write(false)]
	public List<TechSpecItem> SpecItems { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string LocalUnitPriceText => LocalUnitPrice.HasValue ? $"$ {LocalUnitPrice.Value:#,##0.00}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string MSRPText => MSRP.HasValue ? $"$ {MSRP.Value:#,##0.00}" : "-";
	#endregion

	public CPU()
	{
		IOPorts = [];
		SpecItems = [];
	}
}
