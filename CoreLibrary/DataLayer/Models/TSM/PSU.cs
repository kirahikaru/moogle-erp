using Dapper.Contrib.Extensions;
using DataLayer.GlobalConstant;
using MongoDB.Driver.Core.Misc;

namespace DataLayer.Models.TSM;

/// <summary>
/// PSU | Power Supply Unit
/// </summary>
[Table("[vts].[PSU]")]
public class PSU : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(PSU).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"psu";

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
	public string? Dimension { get; set; }
	public string? Material { get; set; }
	/// <summary>
	/// Energy Efficiency Rating
	/// Valid values => EnergyEffRatings
	/// </summary>
	public string? EERating { get; set; }
	public string? Modularity { get; set; }
	public string? ProductInfoUrl { get; set; }
	public string? Note { get; set; }
	public DateTime? ReleaseDate { get; set; }
	public string? ReleaseDateText { get; set; }
	[Range(0, 99999999, ErrorMessage = "'Weight' must be postive.")]
	public decimal? Weight { get; set; }
	[Required(AllowEmptyStrings =true, ErrorMessage = "'Max Power Rating' is required.")]
	[Range(0, 999999, ErrorMessage = "'Power Rating' must be postive whole number.")]
	public int? MaxPowerRating { get; set; }

	public int? ItemId { get; set; }
	[Range(0, 999999999999, ErrorMessage = "'Local Unit Price' must be postive.")]
	public decimal? LocalUnitPrice { get; set; }
	[Range(0, 999999999999, ErrorMessage = "'MSRP' must be postive.")]
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
	[Description("ignore"), ReadOnly(true)]
	public string LocalUnitPriceText => LocalUnitPrice.HasValue ? $"$ {LocalUnitPrice.Value:#,##0.00}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string MSRPText => MSRP.HasValue ? $"$ {MSRP.Value:#,##0.00}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string MaxPowerRatingText => MaxPowerRating.HasValue ? $"{MaxPowerRating.Value}W" : "";

	[Computed, Write(false), ReadOnly(true)]
	public string WeightText => Weight.HasValue ? $"{Weight.Value:#,##0.##}kg" : "";
	#endregion

	public PSU()
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
			"release_date",
			"release_date_text",
			"brand",
			"model",
			"serie_name",
			"dimension",
			"material",
			"eerating",
			"modularity",
			"weight",
			"product_info_url",
			"max_power_rating",
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
			param.Add("@release_date", ReleaseDate);
			param.Add("@release_date_text", ReleaseDateText);
			param.Add("@brand", Brand);
			param.Add("@model", Model);
			param.Add("@serie_name", SerieName);
			param.Add("@dimension", Dimension);
			param.Add("@material", Material);
			param.Add("@eerating", EERating);
			param.Add("@modularity", Modularity);
			param.Add("@weight", Weight);
			param.Add("@product_info_url", ProductInfoUrl);
			param.Add("@max_power_rating", MaxPowerRating);
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
