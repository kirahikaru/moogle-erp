using DataLayer.GlobalConstant;
using DataLayer.Models.TSM;
using Dapper.Contrib.Extensions;
namespace DataLayer.Models.Tech;

/// <summary>
/// HDD | Hard Disk Drive
/// </summary>
[Table("[vts].[HDD]")]
public class HDD : AuditObject
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
	public string? Brand { get; set; }
	public string? SerieName { get; set; }
	public string? ModelNo { get; set; }
	public string? FormFactor { get; set; }
	public int? Capacity { get; set; }
	public string? CapacityUnit { get; set; }

	/// <summary>
	/// MB
	/// </summary>
	public int? CacheSize { get; set; }

	/// <summary>
	/// Disk Speed (RPM)
	/// </summary>
	public int? RotationalSpeed { get; set; }

	/// <summary>
	/// MB/s
	/// </summary>
	public int? TransferRate { get; set; }

	/// <summary>
	/// Recording Technology
	/// </summary>
	public string? RecordingTech { get; set; }

	/// <summary>
	/// Max Sustrained Data Transfer Rate (MB/s)
	/// </summary>
	public int? MaxSustDTR { get; set; }
	public string? Interface { get; set; }
	public string? Certification { get; set; }
	public string? ProductFeatures { get; set; }
	public int? ItemId { get; set; }
	public decimal? LocalUnitPrice { get; set; }
	public decimal? MSRP { get; set; }
	public string? ProductSpecUrl { get; set; }

	/// <summary>
	/// Dimension L x W x H
	/// </summary>
	public string? DimensionLWH { get; set; }

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
	#endregion

	public HDD()
	{
		IOPorts = [];
		SpecItems = [];
	}
}
