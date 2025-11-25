using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.IT;

[Table("[dbo].[ITAssetCategory]"), DisplayName("IT Asset Category")]
public class ITAssetCategory : AuditObject, IParentChildHierarchyObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ITAssetCategory).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "it_asset_category";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Category ID' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Category ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Category Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
    /// <summary>
    /// Valid Values: AssetTypes
    /// </summary>
    [Required(ErrorMessage = "'Asset Type' is required.")]
    public string? AssetType  { get; set; }
    public int? ParentId { get; set; }
    public string? ParentCode { get; set; }
    public string? HierarchyPath { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    /// <summary>
    /// 
    /// </summary>
    [Computed, Write(false)]
    public ITAssetCategory? Parent { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string ObjectNameAndCode => $"{ObjectName.NonNullValue("-")} ({ObjectCode.NonNullValue("-")})";
    #endregion

    public ITAssetCategory() : base()
    {
        
    }
}