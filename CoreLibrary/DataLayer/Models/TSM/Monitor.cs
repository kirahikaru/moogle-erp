using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;
namespace DataLayer.Models.TSM;

/// <summary>
/// PSU | Power Supply Unit
/// </summary>
[Table("[vts].[Monitor]")]
public class Monitor : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(Monitor).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"monitor";

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

	public string? Brand { get; set; }
	public string? ModelName { get; set; }
	public string? ModelYear { get; set; }
	public string? ModelNo { get; set; }
	/// <summary>
	/// UoM: Inch
	/// </summary>
	public decimal? ScreenSize { get; set; }
	/// <summary>
	/// Max Refresh Rate
	/// UoM: Hz
	/// </summary>
	public int? RefreshRate { get; set; }

	/// <summary>
	/// UoM: Nit
	/// </summary>
	public int? Brightness { get; set; }

	/// <summary>
	/// UoM: ms
	/// </summary>
	public decimal? ResponseTime { get; set; }

	public string? PanelType { get; set; }

	public string? ResolutionDesc { get; set; }
	public int? ResHorzPix { get; set; }
	public int? ResVertPix { get; set; }
	public decimal? FullSetWeight { get; set; }
	public decimal? MonitorWeight { get; set; }
	public string? ContrastRatio { get; set; }
	public string? AspectRatio { get; set; }
	public string? Curvature { get; set; }
	public string? Certificates { get; set; }
	public string? DisplayColors { get; set; }
	public bool? HasSpeaker { get; set; }
	public string? PowerConsumption { get; set; }
	public string? ProductSpecUrl { get; set; }
	public decimal? ColorSpaceDCIP3 { get; set; }
	public decimal? ColorSpacesRGB { get; set; }
	public string? OtherSpecDetail { get; set; }
	public string? Remark { get; set; }

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
	public string ResultionText => (ResolutionDesc ?? "") + (ResHorzPix.HasValue ? $" ({ResHorzPix!.Value} x {ResVertPix!.Value})" : "");

	[Computed, Write(false), ReadOnly(true)]
	public string RefreshRateText => RefreshRate.HasValue ? $"{RefreshRate!.Value} Hz" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string ScreenSizeText => ScreenSize.HasValue ? $"{ScreenSize!.Value:#,##0.#}\"" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string FullSetWeightText => FullSetWeight.HasValue ? $"{FullSetWeight!.Value:#,##0.#} kg" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string MonitorWeightText => MonitorWeight.HasValue ? $"{MonitorWeight!.Value:#,##0.#} kg" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string LocalUnitPriceText => LocalUnitPrice.HasValue ? $"$ {LocalUnitPrice.Value:#,##0.00}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string MSRPText => MSRP.HasValue ? $"$ {MSRP.Value:#,##0.00}" : "-";
	#endregion

	public Monitor()
	{
		IOPorts = [];
		SpecItems = [];
	}
}
