using DataLayer.GlobalConstant;

namespace DataLayer.Models.RMS;

[Table("[rms].[Supplier]")]
public class Supplier : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Supplier).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "supplier";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region DATABASE FIELDS 
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Supplier ID' is required.")]
	[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Supplier ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	[MaxLength(80)]
	public new string? ObjectCode { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Name' is required.")]
	[MaxLength(255)]
	public new string? ObjectName { get; set; }
	public string? ObjectNameKh { get; set; }

	[DataType(DataType.Date)]
	public DateTime? OnboardDate { get; set; }

	[DataType(DataType.Date)]
	public DateTime? StartDate { get; set; }

	[DataType(DataType.Date)]
	public DateTime? EndDate { get; set; }

	[DataType(DataType.EmailAddress)]
	[MaxLength(100)]
    public string? Email { get; set; }

    [DataType(DataType.PhoneNumber)]
    [MaxLength(50)]
    public string? PhoneLine1 { get; set; }

	[DataType(DataType.PhoneNumber)]
	[MaxLength(50)]
    public string? PhoneLine2 { get; set; }

    public string? Telegram { get; set; }
    public string? Facebook { get; set; }
    //public string? Youtube { get; set; }
    //public string? TikTok { get; set; }

    [DataType(DataType.Url)]
	public string? Website { get; set; }
    public string? SocialMediaPage { get; set; }
    public string? Note { get; set; }
    public string? LogoImagePath { get; set; }

    /// <summary>
    /// Valid values > GlobalConstants > SupplierStatuses
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string? Status { get; set; }

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false), ReadOnly(true)]
    public string StatusText => SupplierStatuses.GetDisplayText(Status);
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false), ReadOnly(true)]
	public List<Contact> Contacts { get; set; }

	[Computed, Write(false), ReadOnly(true)]
	public List<SupplierBranch> Branches { get; set; }

	[Computed, Write(false), ReadOnly(true)]
	public Address? MainAddress { get; set; }

	[Computed, Write(false), ReadOnly(true)]
	public CambodiaAddress? MainCambodiaAddress { get; set; }

	[Computed, Write(false), ReadOnly(true)]
	public List<ItemSupplier> SaleItems { get; set; }
	#endregion

	public Supplier() : base()
    {
		Contacts = [];
		Branches = [];
		SaleItems = [];
		Status = SupplierStatuses.ACTIVE;
        MainAddress = new();
        MainCambodiaAddress = new();
    }
}