using Dapper.Contrib.Extensions;
using DataLayer.GlobalConstant;
namespace DataLayer.Models.TSM;

/// <summary>
/// PSU | Power Supply Unit
/// </summary>
[Table("[vts].[Motherboard]")]
public class Motherboard : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(Motherboard).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"motherboard";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Record ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Record ID' is required.")]
	public new string? ObjectCode { get; set; }

	public int? ReleaseYear { get; set; }
	public string? ReleaseDateText { get; set; }

	public string? Brand { get; set; }
	public string? Manufacturer { get; set; }

	/// <summary>
	/// Valid Values : GlobalConstants > Motherboard Sockets
	/// </summary>
	public string? CPUSocket { get; set; }

	/// <summary>
	/// Valid Values : GlobalConstants > Form Factors
	/// </summary>
	public string? FormFactor { get; set; }
	public string? Dimension { get; set; }

	public string? Model { get; set; }
	public string? ProductPageUrl { get; set; }
	public int?	ChipsetId { get; set; }
	public string? ChipsetName { get; set; }
	public string? MemorySupport { get; set; }

	public int? ItemId { get; set; }
	public decimal? LocalUnitPrice { get; set; }
	public decimal? MSRP { get; set; }
	public string? Note { get; set; }
	public string? TechSpecInfoUrl { get; set; }
	public string? ProductInfoUrl { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Chipset? Chipset { get; set; }

	[Computed, Write(false)]
	public List<TechSpecItem> SpecItems { get; set; }

	[Computed, Write(false)]
	public List<DeviceIOPort> IOPorts { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string LocalUnitPriceText => LocalUnitPrice.HasValue ? $"$ {LocalUnitPrice.Value:#,##0.00}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string MSRPText => MSRP.HasValue ? $"$ {MSRP.Value:#,##0.00}" : "-";
	#endregion

	public Motherboard()
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
			"chipset_name",
			"chipset_id",
			"cpu_socket",
			"release_year",
			"release_date_text",
			"form_factor",
			"dimension",
			"brand",
			"model",
			"memory_support",
			"manufacturer",
			"item_id",
			"local_unit_price",
			"note",
			"msrp",
			"product_info_url",
			"tech_spec_info_url",
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
			param.Add("@chipset_name", ChipsetName);
			param.Add("@chipset_id", ChipsetId);
			param.Add("@cpu_socket", CPUSocket);
			param.Add("@release_year", ReleaseYear);
			param.Add("@release_date_text", ReleaseDateText);
			param.Add("@form_factor", FormFactor);
			param.Add("@dimension", Dimension);
			param.Add("@brand", Brand);
			param.Add("@model", Model);
			param.Add("@memory_support", MemorySupport);
			param.Add("@manufacturer", Manufacturer);
			param.Add("@item_id", ItemId);
			param.Add("@local_unit_price", LocalUnitPrice);
			param.Add("@note", Note);
			param.Add("@msrp", MSRP);
			param.Add("@product_info_url", ProductInfoUrl);
			param.Add("@tech_spec_info_url", TechSpecInfoUrl);
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
