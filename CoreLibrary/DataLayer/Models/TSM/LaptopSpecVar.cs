using Dapper.Contrib.Extensions;
using DataLayer.GlobalConstant;

namespace DataLayer.Models.TSM;

/// <summary>
/// RAM | Random Access Memory
/// </summary>
[Table("[tsm].[LaptopSpecVar]")]
public class LaptopSpecVar : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(LaptopSpecVar).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"laptop_spec_var";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public int? LaptopId { get; set; }
	public string? LaptopCode { get; set; }
	public int? SeqNo { get; set; }
	public string? Model { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'RAM Size (GB)' is required.")]
	public int? RamSizeGb { get; set; }
	public string? RamType { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'CPU' is required.")]
	public int? CpuId { get; set; }
	public string? CpuName { get; set; }
	public int? GpuId { get; set; }
	public string? GpuName { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Storage' is required.")]
	public int? StorageSizeGb { get; set; }
	public string? StorageType { get; set; }
	public string? Barcode { get; set; }
	public int? ItemId { get; set; }
	public decimal? MSRP { get; set; }
	public string? Note { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYANMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string RamDispTxt => ($"{RamSizeGb.ToText()} {RamType.NonNullValue()}").Trim();

	[Computed, Write(false), ReadOnly(true)]
	public string StorageDispTxt
	{
		get
		{
			StringBuilder sb = new();

			if (StorageSizeGb != null)
			{
				if (StorageSizeGb >= 1024)
					sb.Append($"{Math.Floor(StorageSizeGb.Value / 1024m).ToString("#,##0")} TB");
				else
					sb.Append($"{StorageSizeGb.Value.ToString("#,##0")} GB");
			}

			sb.Append(' ');
			sb.Append(LaptopStorageTypes.GetDisplayText(StorageType));
			return sb.ToString().Trim();
		}
	}
	#endregion

	public LaptopSpecVar()
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
			"laptop_id",
			"laptop_code",
			"seq_no",
			"ram_size_gb",
			"ram_type",
			"cpu_id",
			"cpu_name",
			"gpu_id",
			"gpu_name",
			"storage_size_gb",
			"storage_type",
			"barcode",
			"item_id",
			"is_deleted",
			"created_user",
			"created_datetime",
			"modified_user",
			"modified_datetime" ];
		}
		else
			return [];
	}

	public DynamicParameters GetParamValues(string dbType, bool inclId=false)
	{
		DynamicParameters param = new();

		if (dbType == DatabaseTypes.POSTGRESQL)
		{
			if (inclId)
				param.Add("@id", Id);

			param.Add("@object_name", ObjectName);
			param.Add("@object_code", ObjectCode);
			param.Add("@laptop_id", LaptopId);
			param.Add("@laptop_code", LaptopCode, DbType.AnsiString);
			param.Add("@seq_no", SeqNo);
			param.Add("@ram_size_gb", RamSizeGb);
			param.Add("@ram_type", RamType, DbType.AnsiString);
			param.Add("@cpu_id", CpuId);
			param.Add("@cpu_name", CpuName, DbType.AnsiString);
			param.Add("@gpu_id", GpuId);
			param.Add("@gpu_name", GpuName);
			param.Add("@storage_size_gb", StorageSizeGb);
			param.Add("@storage_type", StorageType, DbType.AnsiString);
			param.Add("@barcode", Barcode, DbType.AnsiString);
			param.Add("@item_id", ItemId);
			param.Add("@is_deleted", IsDeleted, DbType.Boolean);
			param.Add("@created_user", CreatedUser);
			param.Add("@created_datetime", CreatedDateTime);
			param.Add("@modified_user", ModifiedUser);
			param.Add("@modified_datetime", ModifiedDateTime);
		}

		return param;
	}

	#endregion
}
