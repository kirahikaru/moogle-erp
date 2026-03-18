using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;

namespace DataLayer.Models.RMS;

[Table("[rms].[ItemSpec]")]
public class ItemSpec : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ItemSpec).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "item_spec";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? ItemId { get; set; }
	public string? ItemCode { get; set; }
	public int? OrderNo { get; set; }
	public string? SpecTypeCode { get; set; }
	public string? SpecType { get; set; }
	public string? SpecText { get; set; }
	public string? SpecUnit { get; set; }
	public double? SpecValue { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYNAMIC PROPERTY ***

	#endregion

}