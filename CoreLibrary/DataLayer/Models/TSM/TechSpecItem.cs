using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;

namespace DataLayer.Models.TSM;

/// <summary>
/// PSU | Power Supply Unit
/// </summary>
[Table("[vts].[TechSpecItem]")]
public class TechSpecItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(TechSpecItem).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"tech_spec_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public int? OrderNo { get; set; }
	public int? LinkedObjectId { get; set; }
	public string? LinkedObjectType { get; set; }
	public string? UnitCode { get; set; }
	public string? UnitSymbol { get; set; }
	public decimal? SpecValue { get; set; }
	public string? SpecDesc { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Write(false), Computed, ReadOnly(true)]
	public string SpecDisplayValue
	{
		get
		{
			if (!string.IsNullOrEmpty(UnitSymbol) && SpecValue.HasValue)
			{
				return SpecValue.Value.ToString("#,##0.##") + " " + UnitSymbol;
			}
			else
				return string.IsNullOrEmpty(SpecDesc) ? "-" : SpecDesc;
		}
	}
	#endregion

	#region *** DYANMIC PROPERTIES ***
	[Computed, Write(false)]
	public List<TechSpecItem> SpecItems { get; set; }
	#endregion

	public TechSpecItem()
	{
		SpecItems = [];
	}
}
