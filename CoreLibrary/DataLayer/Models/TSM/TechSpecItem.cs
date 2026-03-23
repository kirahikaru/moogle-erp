using Dapper.Contrib.Extensions;
using DataLayer.GlobalConstant;
using DataLayer.Models.RMS;

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

	#region Functions
	public static List<string> GetPgFieldList(string dbType)
	{
		if (dbType == DatabaseTypes.POSTGRESQL)
		{
			return [
			"object_code",
			"object_name",
			"order_no",
			"linked_object_id",
			"linked_object_type",
			"unit_code",
			"unit_symbol",
			"spec_value",
			"spec_desc",
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
			param.Add("@order_no", OrderNo);
			param.Add("@linked_object_id", LinkedObjectId);
			param.Add("@linked_object_type", LinkedObjectType);
			param.Add("@unit_code", UnitCode);
			param.Add("@unit_symbol", UnitSymbol);
			param.Add("@spec_value", SpecValue);
			param.Add("@spec_desc", SpecDesc);
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
