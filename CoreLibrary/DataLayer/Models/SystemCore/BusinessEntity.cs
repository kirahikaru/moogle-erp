using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("BusinessEntity"), DisplayName("Business Entity")]
public class BusinessEntity : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(BusinessEntity).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"business_entity";

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

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
    public string? ObjectNameKh { get; set; }
	/// <summary>
	/// Valid Values > Global Constants > System Core > EntityTypes
	/// C : Company
	/// O : Organization
	/// G : Group
	/// B : Business
	/// R : Restaurant
	/// S : Shop/Store
	/// </summary>
	public string? EntityType { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public string? RegistrationNo { get; set; }
    public string? LicenceNo { get; set; }
    /// <summary>
    /// [FK] > BusinessSector
    /// </summary>
    public int? BusinessSectorId { get; set; }
    public int? IndustryId { get; set; }

    /// <summary>
    /// [FK] Country
    /// </summary>
    public string? BaseCountryCode { get; set; }
    public string? ContactPersonName { get; set; }

    public string? PhoneLine1 { get; set; }
    public string? PhoneLine2 { get; set; }
    public string? PhoneLine3 { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? FacebookPage { get; set; }
    public string? InstagramUser { get; set; }
    public string? LocationUrl { get; set; }
    public int? MainAddressId { get; set; }
    public int? MainCambodiaAddressId { get; set; }
    public string? LogoImagePath { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerObjectCode { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed]
    [Description("ignore")]
    public Country? BaseCountry { get; set; }

    [Computed]
    [Description("ignore")]
    public BusinessSector? BusinessSector { get; set; }

    [Computed]
    [Description("ignore")]
    public Industry? Industry { get; set; }

    [Computed]
    [Description("ignore")]
    public Address? MainAddress { get; set; }

    [Computed]
    [Description("ignore")]
    public CambodiaAddress? MainLocalAddress { get; set; }

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public List<Contact> Contacts { get; set; }
    #endregion

    public BusinessEntity() : base()
    {
        MainAddress = new();
        MainLocalAddress = new();
        Contacts = [];
    }

    #region *** DYNAMIC PROPERTIES ***
    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string RegistrationDateText => RegistrationDate != null ? RegistrationDate!.Value.ToString("dd-MMM-yyyy") : "-";
	#endregion
}