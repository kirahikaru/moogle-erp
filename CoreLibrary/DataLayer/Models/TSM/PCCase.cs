using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;

namespace DataLayer.Models.TSM;

/// <summary>
/// PSU | Power Supply Unit
/// </summary>
[Table("[vts].[PCCase]")]
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
	public string? ModelNo { get; set; }
	public string? ProductSpecUrl { get; set; }

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
	#endregion

	public PCCase()
	{
		IOPorts = [];
		SpecItems = [];
	}
}
