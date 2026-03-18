using DataLayer.GlobalConstant;
using DataLayer.Models.Tech;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Contrib.Extensions;

namespace DataLayer.Models.TSM;

//[Table("tsm.laptop")]
public class Laptop : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(Laptop).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"laptop";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	[Column("brand")]
	public string? Brand { get; set; }
	[Column("model")]
	public string? Model { get; set; }
	[Column("modeldatetext")]
	public string? ModelDateText { get; set; }
	[Column("colors")]
	public string? Colors { get; set; }
	[Column("colors")]
	public string? Series { get; set; }
	[Column("camera")]
	public string? Camera { get; set; }
	[Column("camera")]
	public string? Audio { get; set; }
	[Column("display_spec")]
	public string? DisplaySpec { get; set; }
	[Column("display_size_inch")]
	public decimal? DisplaySizeInch { get; set; }
	[Column("display_resolution")]
	public string? DisplayResolution { get; set; }
	[Column("display_refresh_rate_hz")]
	public int? DisplayRefreshRateHz { get; set; }
	[Column("display_panel_type")]
	public string? DisplayPanelType { get; set; }
	[Column("display_brightness_nits")]
	public int? DisplayBrightnessNits { get; set; }
	[Column("battery")]
	public string? Battery { get; set; }
	[Column("power_supply")]
	public string? PowerSupply { get; set; }
	[Column("weight_kg")]
	public decimal? WeightKg { get; set; }
	[Column("features")]
	public string? Features { get; set; }

	/// <summary>
	/// Network & Communication
	/// </summary>
	[Column("network_and_comm")]
	public string? NetworkAndComm { get; set; }

	/// <summary>
	/// Keyboard & Touchpad
	/// </summary>
	[Column("keyboard_touchpad")]
	public string? KeyboardTouchpad { get; set; }
	[Column("notes")]
	public string? Notes { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false), NotMapped]
	public List<DeviceIOPort> IOPorts { get; set; }

	[Computed, NotMapped]
	public CPU? CPU { get; set; }

	[Computed, NotMapped]
	public GPU? GPU { get; set; }

	[Computed, NotMapped]
	public LaptopMemType? MemoryType { get; set; }

	[Computed, NotMapped]
	public List<TechSpecItem> SpecItems { get; set; }

	[Computed, NotMapped]
	public List<LaptopSpecVar> SpecVariations { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***
	[Computed, NotMapped, Write(false)]
	public string CPUName => CPU != null ? CPU.ObjectName??"-" : "-";

	[Computed, NotMapped, Write(false)]
	public string GPUName => GPU != null ? GPU.ObjectName ?? "-" : "-";

	[Computed, NotMapped, Write(false)]
	public string MemoryTypeName => MemoryType != null ? MemoryType.SpecCode ?? "-" : "-";
	#endregion

	public Laptop()
	{
		IOPorts = [];
		SpecItems = [];
		SpecVariations = [];
	}

	#region Functions
	public static List<string> GetPgFieldList(string dbType)
	{
		if (dbType == DatabaseTypes.POSTGRESQL)
		{
			return [
			"object_code",
			"object_name",
			"model",
			"modeldatetext",
			"colors",
			"brand",
			"series",
			"camera",
			"audio",
			"display_size_inch",
			"display_resolution",
			"display_panel_type",
			"display_refresh_rate_hz",
			"display_brightness_nits",
			"battery",
			"power_supply",
			"weight_kg",
			"network_and_comm",
			"keyboard_touchpad",
			"features",
			"notes",
			"is_deleted",
			"created_user",
			"created_datetime",
			"modified_user",
			"modified_datetime" ];
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

			param.Add("@object_name", ObjectName);
			param.Add("@object_code", ObjectCode);
			param.Add("@model", Model, DbType.AnsiString);
			param.Add("@modeldatetext", ModelDateText, DbType.AnsiString);
			param.Add("@colors", Colors, DbType.AnsiString);
			param.Add("@brand", Brand, DbType.AnsiString);
			param.Add("@series", Series, DbType.AnsiString);
			param.Add("@camera", Series, DbType.AnsiString);
			param.Add("@audio", Audio, DbType.AnsiString);
			param.Add("@display_spec", DisplaySpec, DbType.AnsiString);
			param.Add("@display_size_inch", DisplaySizeInch);
			param.Add("@display_resolution", DisplayResolution, DbType.AnsiString);
			param.Add("@display_panel_type", DisplayPanelType, DbType.AnsiString);
			param.Add("@display_refresh_rate_hz", DisplayRefreshRateHz);
			param.Add("@display_brightness_nits", DisplayBrightnessNits);
			param.Add("@battery", Battery, DbType.AnsiString);
			param.Add("@power_supply", PowerSupply, DbType.AnsiString);
			param.Add("@weight_kg", WeightKg);
			param.Add("@network_and_comm", NetworkAndComm);
			param.Add("@keyboard_touchpad", KeyboardTouchpad);
			param.Add("@features", Features);
			param.Add("@notes", Notes);
			param.Add("@is_deleted", IsDeleted, DbType.Boolean);
			param.Add("@created_user", CreatedUser);
			param.Add("@created_datetime", CreatedDateTime);
			param.Add("@modified_user", ModifiedUser);
			param.Add("@modified_datetime", ModifiedDateTime);
		}

		return param;
	}
	#endregion
}
