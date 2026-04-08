using Dapper.Contrib.Extensions;
using DataLayer.GlobalConstant;
using MongoDB.Driver.Core.Misc;

namespace DataLayer.Models.TSM;

/// <summary>
/// PSU | Power Supply Unit
/// </summary>
[Table("[vts].[PCCase]"), DisplayName("PC Case")]
public class PCCase : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(PCCase).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"pc_case";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public string? Brand { get; set; }
	public string? Model { get; set; }
	public DateTime? ReleaseDate { get; set; }
	public string? ReleaseDateText { get; set; }
	public string? Material { get; set; }
	public string? CaseType { get; set; }
	public string? MotherboardSupport { get; set; }
	public decimal? WeightKg { get; set; }
	public decimal? Width { get; set; }
	public decimal? Height { get; set; }
	public decimal? Depth { get; set; }
	public string? SizeUnit { get; set; }
	public string? TechSpecInfoUrl { get; set; }
	public string? ProductInfoUrl { get; set; }
	public string? Note { get; set; }
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
	[Computed, Write(false), ReadOnly(true)]
	public string LocalUnitPriceText => LocalUnitPrice.HasValue ? $"$ {LocalUnitPrice.Value:#,##0.00}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string MSRPText => MSRP.HasValue ? $"$ {MSRP.Value:#,##0.00}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string WeightKgText => WeightKg.HasValue ? $"{WeightKg.Value:#,##0.##}Kg" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string SizeText
	{
		get
		{
			StringBuilder sb = new();
			sb.Append("WxHxD: ");

			if (Width.HasValue)
				sb.Append(Width!.Value.ToString("#,##0.##") + "x");
			else
				sb.Append(" - x");

			if (Height.HasValue)
				sb.Append(Height!.Value.ToString("#,##0.##") + "x");
			else
				sb.Append(" - x");

			if (Depth.HasValue)
				sb.Append(Depth!.Value.ToString("#,##0.##"));
			else
				sb.Append(" - ");

			sb.Append(SizeUnit.NonNullValue(""));

			return sb.ToString();
		}
	}
	#endregion

	public PCCase()
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
			"material",
			"case_type",
			"motherboard_support",
			"weight_kg",
			"width",
			"height",
			"depth",
			"size_unit",
			"tech_spec_info_url",
			"product_info_url",
			"note",
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
			param.Add("@material", Material);
			param.Add("@case_type", CaseType);
			param.Add("@motherboard_support", MotherboardSupport);
			param.Add("@weight_kg", WeightKg);
			param.Add("@width", Width);
			param.Add("@height", Height);
			param.Add("@depth", Depth);
			param.Add("@size_unit", SizeUnit);
			param.Add("@tech_spec_info_url", TechSpecInfoUrl);
			param.Add("@product_info_url", ProductInfoUrl);
			param.Add("@note", Note);
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
