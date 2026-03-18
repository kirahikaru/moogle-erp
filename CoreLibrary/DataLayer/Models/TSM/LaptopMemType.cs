using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;
namespace DataLayer.Models.TSM;

/// <summary>
/// RAM | Random Access Memory
/// </summary>
[Table("[vts].[RAMType]")]
public class LaptopMemType : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"RAMType";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"ram_type";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***

	public string? SpecCode { get; set; }
	public string? MemorySpeed { get; set; }

	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYANMIC PROPERTIES ***

	#endregion

	public LaptopMemType()
	{
		
	}
}
