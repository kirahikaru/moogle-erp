using Dapper.Contrib.Extensions;
using DataLayer.GlobalConstant;
using DataLayer.Models.RMS;
using MongoDB.Driver.Core.Misc;
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
	public int? SeqNo { get; set; }
	public int? LinkedObjectId { get; set; }
	public string? LinkedObjectCode { get; set; }
	public string? LinkedObjectType { get; set; }
	public int? PortCount { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYANMIC PROPERTIES ***

	#endregion

	public DeviceIOPort()
	{

	}

	#region Functions
	public static List<string> GetPgFieldList(string dbType)
	{
		if (dbType == DatabaseTypes.POSTGRESQL)
		{
			return [
			"object_code",
			"object_name",
			"seq_no",
			"linked_object_id",
			"linked_object_code",
			"linked_object_type",
			"port_count",
			"remark",
			"is_deleted",
			"created_user",
			"created_datetime",
			"modified_user",
			"modified_datetime"
			];
		}
		else
			return [];
	}

	public DynamicParameters GetParamValues(string dbType, bool inclId = false)
	{
		DynamicParameters param = new();

		if (dbType == DatabaseTypes.POSTGRESQL)
		{
			if (inclId)
				param.Add("@id", Id);

			param.Add("@object_code", ObjectCode);
			param.Add("@object_name", ObjectName);
			param.Add("@seq_no", SeqNo);
			param.Add("@linked_object_id", LinkedObjectId);
			param.Add("@linked_object_code", LinkedObjectCode);
			param.Add("@linked_object_type", LinkedObjectType);
			param.Add("@port_count", PortCount);
			param.Add("@remark", Remark);
			param.Add("@is_deleted", IsDeleted);
			param.Add("@created_user", CreatedUser);
			param.Add("@created_datetime", CreatedDateTime);
			param.Add("@modified_user", ModifiedUser);
			param.Add("@modified_datetime", ModifiedDateTime);
		}

		return param;
	}
	#endregion
}
