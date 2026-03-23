using Dapper.Contrib.Extensions;
using DataLayer.GlobalConstant;

namespace DataLayer.Models.TSM;

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
	public string? CapacityText { get; set; }
	public int? CapacityGb { get; set; }
	public string? MultiChannelKit { get; set; }
	public string? Brand { get; set; }
	public string? SerieName { get; set; }
	public string? Model { get; set; }
	public string? MemoryType { get; set; }
	public string? OCProfileSupport { get; set; }
	public string? ECC { get; set; }

	/// <summary>
	/// Unit: MT/s
	/// Tested Speed (Up To) (XMP)
	/// </summary>
	public int? TestedSpeed { get; set; }
	public string? TestedLatency { get; set; }

	/// <summary>
	/// Unit: V
	/// </summary>
	public decimal? TestedVoltage { get; set; }
	/// <summary>
	/// SPD Speed MHz
	/// </summary>
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
	public DateTime? ReleaseDate { get; set; }
	public string? ReleaseDateText { get; set; }
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

	[Computed, Write(false), ReadOnly(true)]
	public string SPDSpeedText => SPDSpeed.HasValue ? $"{SPDSpeed}MHz" : "";
	//{
	//	get
	//	{
	//		StringBuilder sb = new();

	//		if (SPDSpeed.HasValue)
	//		{
	//			if (SPDSpeed > 1000)
	//			{
	//				sb.Append($"{Math.Floor(SPDSpeed.Value / 1000M):#,##0}GHz");
	//			}
	//			else
	//				sb.Append($"{SPDSpeed.Value:#,##0}MHz");
	//		}

	//		return sb.ToString();
	//	}
	//}
	#endregion

	public RAM()
	{
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
			"spec_code",
			"memory_type",
			"capacity_text",
			"capacity_gb",
			"multichannel_kit",
			"brand",
			"model",
			"series_name",
			"oc_profile_support",
			"tested_speed",
			"tested_latency",
			"tested_voltage",
			"ecc",
			"spd_speed",
			"spd_voltage",
			"is_fan_included",
			"features",
			"product_info_url",
			"item_id",
			"local_unit_price",
			"msrp",
			"release_date",
			"release_date_text",
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
			param.Add("@spec_code", SpecCode);
			param.Add("@memory_type", MemoryType);
			param.Add("@capacity_text", CapacityText);
			param.Add("@capacity_gb", CapacityGb);
			param.Add("@multichannel_kit", MultiChannelKit);
			param.Add("@brand", Brand);
			param.Add("@model", Model);
			param.Add("@series_name", SerieName);
			param.Add("@oc_profile_support", OCProfileSupport);
			param.Add("@tested_speed", TestedSpeed);
			param.Add("@tested_latency", TestedLatency);
			param.Add("@tested_voltage", TestedVoltage);
			param.Add("@ecc", ECC);
			param.Add("@spd_speed", SPDSpeed);
			param.Add("@spd_voltage", SPDVoltage);
			param.Add("@is_fan_included", IsFanIncluded);
			param.Add("@features", Features);
			param.Add("@product_info_url", ProductInfoUrl);
			param.Add("@item_id", ItemId);
			param.Add("@local_unit_price", LocalUnitPrice);
			param.Add("@msrp", MSRP);
			param.Add("@release_date", ReleaseDate);
			param.Add("@release_date_text", ReleaseDateText);
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
