using DataLayer.GlobalConstant;

namespace DataLayer.Models.HomeInventory;
[Table("[home].[Merchant]")]
public class Merchant : AuditObject, IParentChildHierarchyObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOME_INVENTORY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Merchant).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "merchant";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'CODE' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }
    public string? ObjectNameKh { get; set; }
    /// <summary>
    /// GlobalConstant.MerchantTypes
    /// </summary>
	public string? MerchantType { get; set; }
    public int? AddrId { get; set; }
    public int? KhAddrId { get; set; }
    public string? Link { get; set; }
    public int? ParentId { get; set; }
    public string? ParentCode { get; set; }

    [MaxLength(255)]
    public string? HierarchyPath { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Address? Address { get; set; }

    [Computed, Write(false)]
    public CambodiaAddress? CambodiaAddress { get; set; }

	[Computed, Write(false)]
	public Merchant? Parent { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string MerchantTypeText => MerchantTypes.GetDisplayText(MerchantType);
	#endregion
}