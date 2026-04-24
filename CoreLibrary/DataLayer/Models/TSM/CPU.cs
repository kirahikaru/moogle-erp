using Dapper.Contrib.Extensions;
using DataLayer.GlobalConstant;
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

	public string? Brand { get; set; }
	public string? Manufacturer { get; set; }
	public string? CodeName { get; set; }
	public string? FullTrademarkName { get; set; }
	
	public string? Family { get; set; }
	public DateTime? LaunchDate { get; set; }
	public string? LaunchDateText { get; set; }
	
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
	public int? L1CacheSizeKb { get; set; }
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
	public int? MaxOpTemp { get; set; }
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
	public string? TechSpecInfoUrl { get; set; }
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

	[Computed, Write(false), ReadOnly(true)]
	public string FormFactorText => CPUFormFactors.GetDisplayText(FormFactor);

	[Computed, Write(false), ReadOnly(true)]
	public string BaseClockSpeedText => BaseClockSpeedGHz.HasValue ? $"{BaseClockSpeedGHz:#,##0.##}GHz" : "";
	[Computed, Write(false), ReadOnly(true)]
	public string MaxClockSpeedText => MaxClockSpeedGHz.HasValue ? $"Up to {MaxClockSpeedGHz:#,##0.##}GHz" : "";
	#endregion

	public CPU()
	{
		IOPorts = [];
		SpecItems = [];
	}

	#region Functions
	public static List<string> GetPgFieldList(string dbType)
	{
		if (dbType == DatabaseTypes.POSTGRESQL)
		{
			return [
			"object_code",
			"object_name",
			"brand",
			"manufacturer",
			"code_name",
			"full_trademark_name",
			"family",
			"launch_date",
			"launch_date_text",
			"series_name",
			"form_factor",
			"core_count",
			"thread_count",
			"max_clock_speed_ghz",
			"base_clock_speed_ghz",
			"l1_cache_size_kb",
			"l2_cache_size_mb",
			"l3_cache_size_mb",
			"default_tdp_watt",
			"is_unlocked_for_oc",
			"cpu_socket",
			"thermal_soln_pib",
			"thermal_soln_mpk",
			"recommended_cooler",
			"max_op_temp",
			"os_support",
			"pci_express_ver",
			"sys_mem_type",
			"memory_channel",
			"integrated_gpu",
			"processor_base_power",
			"max_turbo_power",
			"sys_mem_spec",
			"product_info_url",
			"tech_spec_info_url",
			"item_id",
			"local_unit_price",
			"msrp",
			"is_deleted",
			"created_user",
			"created_datetime",
			"modified_user",
			"modified_datetime" 
			];
		}
		else
			return [];
	}

	public DynamicParameters GetParamValues(string dbType, bool inclId = false)
	{
		DynamicParameters param = new();

		if (dbType == DatabaseTypes.POSTGRESQL)
		{
			if (inclId)
				param.Add("@id", Id);

			param.Add("@object_code", ObjectCode);
			param.Add("@object_name", ObjectName);
			param.Add("@brand", Brand);
			param.Add("@manufacturer", Manufacturer);
			param.Add("@code_name", CodeName);
			param.Add("@full_trademark_name", FullTrademarkName);
			param.Add("@family", Family);
			param.Add("@launch_date", LaunchDate);
			param.Add("@launch_date_text", LaunchDateText);
			param.Add("@series_name", SeriesName);
			param.Add("@form_factor", FormFactor);
			param.Add("@core_count", CoreCount);
			param.Add("@thread_count", ThreadCount);
			param.Add("@max_clock_speed_ghz", MaxClockSpeedGHz);
			param.Add("@base_clock_speed_ghz", BaseClockSpeedGHz);
			param.Add("@l1_cache_size_kb", L1CacheSizeKb);
			param.Add("@l2_cache_size_mb", L2CacheSizeMb);
			param.Add("@l3_cache_size_mb", L3CacheSizeMb);
			param.Add("@default_tdp_watt", DefaultTDPWatt);
			param.Add("@is_unlocked_for_oc", IsUnlockedForOC);
			param.Add("@cpu_socket", CPUSocket);
			param.Add("@thermal_soln_pib", ThermalSolutionPIB);
			param.Add("@thermal_soln_mpk", ThermalSolutionMPK);
			param.Add("@recommended_cooler", RecommendedCooler);
			param.Add("@max_op_temp", MaxOpTemp);
			param.Add("@os_support", OSSupport);
			param.Add("@pci_express_ver", PCIExpressVer);
			param.Add("@sys_mem_type", SysMemType);
			param.Add("@memory_channel", MemoryChannel);
			param.Add("@integrated_gpu", IntegratedGPU);
			param.Add("@processor_base_power", ProcessorBasePower);
			param.Add("@max_turbo_power", MaxTurboPower);
			param.Add("@sys_mem_spec", SysMemSpec);
			param.Add("@product_info_url", ProductInfoUrl);
			param.Add("@tech_spec_info_url", TechSpecInfoUrl);
			param.Add("@item_id", ItemId);
			param.Add("@local_unit_price", LocalUnitPrice);
			param.Add("@msrp", MSRP);
			param.Add("@is_deleted", IsDeleted);
			param.Add("@created_user", CreatedUser);
			param.Add("@created_datetime", CreatedDateTime);
			param.Add("@modified_user", ModifiedUser);
			param.Add("@modified_datetime", ModifiedDateTime);
		}

		return param;
	}
	#endregion
}
