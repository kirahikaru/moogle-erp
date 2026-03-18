using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;
namespace DataLayer.Models.TSM;

[Table("[vts].[IOPort]")]
public class DeviceIOPort : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"IOPort";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"io_port";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***

	public int? LinkedObjectId { get; set; }
	public string? LinkedObjectType { get; set; }
	public int? PortCount { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYANMIC PROPERTIES ***

	#endregion

	public DeviceIOPort()
	{

	}
}
