using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;
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

	public int? ReleasedYear { get; set; }

	public string? Brand { get; set; }

	/// <summary>
	/// Valid Values : GlobalConstants > Motherboard Sockets
	/// </summary>
	public string? Socket { get; set; }

	/// <summary>
	/// Valid Values : GlobalConstants > Form Factors
	/// </summary>
	public string? FormFactor { get; set; }

	public string? ModelName { get; set; }
	public string? ProductPageUrl { get; set; }
	public int?	ChipsetId { get; set; }
	public string? MemorySupport { get; set; }

	public int? ItemId { get; set; }
	public decimal? LocalUnitPrice { get; set; }
	public decimal? MSRP { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Chipset? Chipset { get; set; }

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
	}
}
