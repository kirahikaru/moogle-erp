using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("ProductCategory")]
public class ProductCategory : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(ProductCategory).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"product_category";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ObjectNameKh { get; set; }
    public string? CategoryTypeCode { get; set; }

    public int? OrgId { get; set; }
    public int? OrgBranchId { get; set; }
    public bool IsEnabled { get; set; }
    public string? Remark { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}