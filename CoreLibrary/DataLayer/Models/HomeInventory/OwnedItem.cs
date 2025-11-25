using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SysCore;
using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.HomeInventory;

[Table("[home].[OwnedItem]"), DisplayName("My Item")]
public class OwnedItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOME_INVENTORY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(OwnedItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "owned_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	[Required(AllowEmptyStrings = false, ErrorMessage = "'CODE' is required.")]
    public new string? ObjectCode { get; set; }

    [RegularExpression(@"^[a-zA-Z\d\s\W]{0,}$", ErrorMessage = "'Name (En)' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "'Name (En)' is required.")]
    public string? NameEn { get; set; }
	public string? NameKh { get; set; }
	public int? OwnedItemCategoryId { get; set; }

	[RegularExpression(@"^[A-Z0-9]{0,}$", ErrorMessage = "'Barcode' specified is of invalid format.")]
	[MaxLength(25)]
	public string? Barcode { get; set; }

	[MaxLength(150)]
	public string? Brand { get; set; }

	[MaxLength(30)]
	public string? ModelNo { get; set; }

	[MaxLength(50)]
	public string? SerialNumber { get; set; }

	[MaxLength(80)]
	public string? OtherRefNum1 { get; set; }

	[MaxLength(80)]
	public string? OtherRefNum2 { get; set; }

	[MaxLength(255), StringUnicode(true)]
	public string? ItemDescription { get; set; }

	[MaxLength(255)]
	public string? Specification { get; set; }

	[MaxLength(255), StringUnicode(true)]
	public string? Remark { get; set; }

	/// <summary>
	/// ValidValues > GlobalConstant.ObjectStates
	/// Controller > GlobalConstant.OwnedItemStateController
	/// </summary>
	[Required(ErrorMessage = "'Object State' is required.")]
	[MaxLength(50)]
	public string? ObjectState { get; set; }

	public DateTime? PurchasedDate { get; set; }
	public int? PurchasedLocationId { get; set; }

	[DataType(DataType.Currency)]
	[Range(0, 9999999999999999.00, ErrorMessage = "'Purchased Price' Must be positive amount.")]
	[Precision(18, 2)]
	public double? PurchasedPrice { get; set; }

	[DataType(DataType.Currency)]
	[Range(0, 9999999999999999.00, ErrorMessage = "'Other Cost' Must be positive amount.")]
	[Precision(18, 2)]
	public double? OtherCost { get; set; }
	public int? Quantity { get; set; }

	public string? MerchantObjectCode { get; set; }

	[MaxLength(3)]
	public string? ManufacturerCountryCode { get; set; }

	[MaxLength(255)]
	public string? ImagePath { get; set; }

	[MaxLength(255)]
	public string? ReferenceLink { get; set; }
    [MaxLength(255)]
    public string? PurchaseLink { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public OwnedItemCategory? Category { get; set; }

	[Computed, Write(false)]
	public Merchant? Merchant { get; set; }

	[Computed, Write(false)]
	public Country? ManufacturedCountry { get; set; }

	[Computed, Write(false)]
	public List<ObjectStateHistory> AuditTrails { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***
	
	#endregion

	public OwnedItem()
	{
		ObjectState = ObjectStates.NEW;
		AuditTrails = [];
	}
}