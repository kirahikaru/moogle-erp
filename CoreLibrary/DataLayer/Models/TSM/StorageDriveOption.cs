using Dapper.Contrib.Extensions;
using DataLayer.GlobalConstant;
namespace DataLayer.Models.TSM;

/// <summary>
/// HDD | Hard Disk Drive
/// </summary>
[Table("[vts].[StorageDriveOption]")]
public class StorageDriveOption : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.TECH_STORE;

	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(StorageDriveOption).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"storage_drive_option";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public int? SeqNo { get; set; }
	public int? Capacity { get; set; }
	public string? StorageType { get; set; }
	public int? StorageDriveId { get; set; }
	public string? CapacityUnit { get; set; }
	public int? ItemId { get; set; }
	public string? Colors { get; set; }
	public string? Region { get; set; }
	public string? Warranty { get; set; }
	public string? Barcode { get; set; }
	public string? BarcodeUPC { get; set; }
	public string? BarcodeEAN { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public List<DeviceIOPort> IOPorts { get; set; }

	[Computed, Write(false)]
	public List<TechSpecItem> SpecItems { get; set; }

	[Computed, Write(false)]
	public List<StorageDriveOption> StorageOptions { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***

	#endregion

	public StorageDriveOption()
	{
		IOPorts = [];
		SpecItems = [];
		StorageOptions = [];
	}

	#region Functions
	public static List<string> GetPgFieldList(string dbType)
	{
		if (dbType == DatabaseTypes.POSTGRESQL)
		{
			return [
			"object_code",
			"object_name",
			"storage_type",
			"storage_drive_id",
			"seq_no",
			"capacity",
			"capacity_unit",
			"item_id",
			"colors",
			"region",
			"warranty",
			"barcode",
			"barcode_upc",
			"barcode_ean",
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
			param.Add("@storage_type", StorageType);
			param.Add("@storage_drive_id", StorageDriveId);
			param.Add("@seq_no", SeqNo);
			param.Add("@capacity", Capacity);
			param.Add("@capacity_unit", CapacityUnit);
			param.Add("@item_id", ItemId);
			param.Add("@colors", Colors);
			param.Add("@region", Region);
			param.Add("@warranty", Warranty);
			param.Add("@barcode", Barcode);
			param.Add("@barcode_upc", BarcodeUPC);
			param.Add("@barcode_ean", BarcodeEAN);
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
