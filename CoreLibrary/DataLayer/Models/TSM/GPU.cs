using Dapper.Contrib.Extensions;
using DataLayer.GlobalConstant;
using DataLayer.Models.RMS;

namespace DataLayer.Models.TSM;

/// <summary>
/// GPU | Graphic Processing Unit
/// </summary>
[Table("[vts].[GPU]")]
public class GPU : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(GPU).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"gpu";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public string? Brand { get; set; }
	public string? Model { get; set; }
	public string? GraphicEngine { get; set; }
	public string? GPUName { get; set; }
	public string? GPUVariant { get; set; }
	public string? ProcessType { get; set; }
	public string? ProcessSize { get; set; }
	public int? MemorySizeGb { get; set; }
	public int? MemoryBusBit { get; set; }
	/// <summary>
	/// Bandwidth (GB/s)
	/// </summary>
	public decimal? Bandwidth { get; set; }
	public string? MemoryType { get; set; }
	public string? BusInterface { get; set; }

	public DateTime? ReleaseDate { get; set; }
	public string? ReleaseDateText { get; set; }
	public int? MemoryClockMhz { get; set; }
	public int? BaseClockMhz { get; set; }
	public int? BoostClockMhz { get; set; }
	public string? CUDA { get; set; }

	public string? MicrosoftDirectX { get; set; }
	public string? OpenGL { get; set; }
	public string? Resolution { get; set; }
	public string? Dimension { get; set; }
	public int? ShadingUnitCoreCount { get; set; }
	/// <summary>
	/// Texture Mapping Unit
	/// </summary>
	public int? TMU { get; set; }
	/// <summary>
	/// Render Output Units (ROPs)
	/// </summary>
	public int? ROP { get; set; }
	public int? L1CacheKb { get; set; }
	public int? L2CacheMb { get; set; }
	public int? RecPSUPower { get; set; }
	public string? PowerConnector { get; set; }
	public string? Software { get; set; }
	public string? ProductInfoUrl { get; set; }
	public string? TechSpecInfoUrl { get; set; }
	public int? ItemId { get; set; }
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
	[Computed, Write(false)]
	public string MemoryDispText
	{
		get
		{
			StringBuilder sb = new();

			if (MemorySizeGb.HasValue)
			{
				if (MemorySizeGb >= 1024)
				{
					sb.Append(((int)Math.Floor(MemorySizeGb!.Value / 1024M)).ToString() + " TB ");
				}
				else
					sb.Append($"{MemorySizeGb:#,##0}GB ");

				sb.Append(MemoryType);
			}

			return sb.ToString();
		}
	}

	[Computed, Write(false)]
	[Description("ignore"), ReadOnly(true)]
	public string LocalUnitPriceText => LocalUnitPrice.HasValue ? $"$ {LocalUnitPrice.Value:#,##0.00}" : "-";

	[Computed, Write(false)]
	[Description("ignore"), ReadOnly(true)]
	public string MSRPText => MSRP.HasValue ? $"$ {MSRP.Value:#,##0.00}" : "-";
	#endregion

	public GPU()
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
			"model",
			"graphic_engine",
			"gpu_name",
			"gpu_variant",
			"process_type",
			"process_size",
			"memory_size_gb",
			"memory_bus_bit",
			"bandwidth",
			"memory_type",
			"bus_interface",
			"release_date",
			"release_date_text",
			"memory_clock_mhz",
			"base_clock_mhz",
			"boost_clock_mhz",
			"microsoft_direct_x",
			"open_gl",
			"cuda",
			"resolution",
			"dimension",
			"rec_psu_power",
			"power_connector",
			"software",
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
			param.Add("@model", Model);
			param.Add("@graphic_engine", GraphicEngine);
			param.Add("@gpu_name", GPUName);
			param.Add("@gpu_variant", GPUVariant);
			param.Add("@process_type", ProcessType);
			param.Add("@process_size", ProcessSize);
			param.Add("@memory_size_gb", MemorySizeGb);
			param.Add("@memory_bus_bit", MemoryBusBit);
			param.Add("@bandwidth", Bandwidth);
			param.Add("@memory_type", MemoryType);
			param.Add("@bus_interface", BusInterface);
			param.Add("@release_date", ReleaseDate);
			param.Add("@release_date_text", ReleaseDateText);
			param.Add("@memory_clock_mhz", MemoryClockMhz);
			param.Add("@base_clock_mhz", BaseClockMhz);
			param.Add("@boost_clock_mhz", BoostClockMhz);
			param.Add("@microsoft_direct_x", MicrosoftDirectX);
			param.Add("@open_gl", OpenGL);
			param.Add("@cuda", CUDA);
			param.Add("@resolution", Resolution);
			param.Add("@dimension", Dimension);
			param.Add("@rec_psu_power", RecPSUPower);
			param.Add("@power_connector", PowerConnector);
			param.Add("@software", Software);
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