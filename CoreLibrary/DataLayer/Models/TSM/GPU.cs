using DataLayer.GlobalConstant;
using DataLayer.Models.TSM;
using Dapper.Contrib.Extensions;

namespace DataLayer.Models.Tech;

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
	public string? GraphicEngine { get; set; }
	public string? Brand { get; set; }
	public string? Model { get; set; }
	public string? BusStandard { get; set; }
	public int? MemoryAmount { get; set; }
	public string? MemoryInterface { get; set; }
	public string? DRAMType { get; set; }
	public int? CUDACore { get; set; }
	public int? GraphicsClock { get; set; }
	public int? BoostClock { get; set; }
	public string? MicrosoftDirectX { get; set; }
	public string? OpenGL { get; set; }
	public string? Resolution { get; set; }
	public string? Dimension { get; set; }
	public int? RecommendedPSUPower { get; set; }
	public string? PowerConnector { get; set; }
	public string? Software { get; set; }
	public string? ProductInfoUrl { get; set; }
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
	public string MemoryDisplayText => MemoryAmount != null ? $"{MemoryAmount} GB ({DRAMType ?? ""})" : "-";

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
}
