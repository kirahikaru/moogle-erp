using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;

namespace DataLayer.Models.TSM;

/// <summary>
/// SSD | Solid-State Drives
/// </summary>
[Table("[vts].[SSD]")]
public class SSD : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(SSD).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"ssd";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public string? Brand { get; set; }
	public string? SerieName { get; set; }
	public string? ModelNo { get; set; }
	public string? FormFactor { get; set; }
	public int? Capacity { get; set; }
	public string? CapacityUnit { get; set; }

	/// <summary>
	/// MB/s
	/// </summary>
	public int? MaxSeqReadSpeed { get; set; }
	/// <summary>
	/// MB/s
	/// </summary>
	public int? MaxSeqWriteSpeed { get; set; }
	public string? ProductSpecUrl { get; set; }
	public int? ItemId { get; set; }

	/// <summary>
	/// Unit: $
	/// </summary>
	public decimal? LocalUnitPrice { get; set; }
	/// <summary>
	/// Manufacturer's Suggested Retail Price (MSRP)
	/// Unit: $
	/// </summary>
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
	public string MaxSeqReadSpeedText => MaxSeqReadSpeed.HasValue ? $"Up to {MaxSeqReadSpeed.Value:#,##0} MB/s" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string MaxSeqWriteSpeedText => MaxSeqWriteSpeed.HasValue ? $"Up to {MaxSeqWriteSpeed.Value:#,##0} MB/s" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string LocalUnitPriceText => LocalUnitPrice.HasValue ? $"$ {LocalUnitPrice.Value:#,##0.00}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string MSRPText => MSRP.HasValue ? $"$ {MSRP.Value:#,##0.00}" : "-";
	#endregion

	public SSD()
	{
		IOPorts = [];
		SpecItems = [];
	}
}
