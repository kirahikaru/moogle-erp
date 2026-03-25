using Dapper.Contrib.Extensions;
using DataLayer.GlobalConstant;
using DataLayer.Models.RMS;
namespace DataLayer.Models.TSM;

/// <summary>
/// HDD | Hard Disk Drive
/// </summary>
[Table("[vts].[HDD]")]
public class HDD : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(HDD).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"hdd";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***

	public string? Brand { get; set; }
	public string? Model { get; set; }
	public string? SerieName { get; set; }
	public DateTime? ReleaseDate { get; set; }
	public string? ReleaseDateText { get; set; }
	public string? DimensionLWH { get; set; }
	public string? FormFactor { get; set; }
	public int? CacheSizeMB { get; set; }
	public int? RotationalSpeed { get; set; }
	public int? TransferRate { get; set; }
	public string? RecordingTech { get; set; }
	public int? MaxSustDTR { get; set; }
	public string? Interface { get; set; }
	public string? Certification { get; set; }
	public string? ProductFeatures { get; set; }
	public string? Note { get; set; }
	public string? TechSpecInfoUrl { get; set; }
	public string? ProductInfoUrl { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public List<DeviceIOPort> IOPorts { get; set; }

	[Computed, Write(false)]
	public List<TechSpecItem> SpecItems { get; set; }

	[Computed, Write(false)]
	public List<StorageDriveOption> Options { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***

	#endregion

	public HDD()
	{
		IOPorts = [];
		SpecItems = [];
		Options = [];
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
			"serie_name",
			"release_date",
			"release_date_text",
			"dimension_lwh",
			"form_factor",
			"cache_size_mb",
			"rotational_speed",
			"transfer_rate",
			"recording_tech",
			"max_sust_dtr",
			"interface",
			"certification",
			"product_features",
			"note",
			"tech_spec_info_url",
			"product_info_url",
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
			param.Add("@serie_name", SerieName);
			param.Add("@release_date", ReleaseDate);
			param.Add("@release_date_text", ReleaseDateText);
			param.Add("@dimension_lwh", DimensionLWH);
			param.Add("@form_factor", FormFactor);
			param.Add("@cache_size_mb", CacheSizeMB);
			param.Add("@rotational_speed", RotationalSpeed);
			param.Add("@transfer_rate", TransferRate);
			param.Add("@recording_tech", RecordingTech);
			param.Add("@max_sust_dtr", MaxSustDTR);
			param.Add("@interface", Interface);
			param.Add("@certification", Certification);
			param.Add("@product_features", ProductFeatures);
			param.Add("@note", Note);
			param.Add("@tech_spec_info_url", TechSpecInfoUrl);
			param.Add("@product_info_url", ProductInfoUrl);
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
