using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataAnnot = System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Models.SysCore;

[Table("Person")]
public class Person : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(Person).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"person";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	/// <summary>
	/// a.k.a LastName
	/// </summary>
	[RegularExpression(@"^[a-zA-Z.,-]{0,}$", ErrorMessage = "'Last Name' invalid format.")]
    [MaxLength(150)]
    public string? Surname { get; set; }

    [MaxLength(150), StringUnicode(true)]
    public string? ObjectNameKh { get; set; }

    [MaxLength(150), StringUnicode(true)]
    public string? SurnameKh { get; set; }

    public string? MiddleName { get; set; }

    /// <summary>
    /// a.k.a FirstName
    /// </summary>
    [MaxLength(150)]
    public string? GivenName { get; set; }

    [MaxLength(150), StringUnicode(true)]
    public string? GivenNameKh { get; set; }

    public DateTime? BirthDate { get; set; }
    public DateTime? DeathDate { get; set; }
    /// <summary>
    /// Valid Values from GlobalConstants.Genders
    /// </summary>
    [Required(ErrorMessage = "'Gender' is required.")]
    [MaxLength(1)]
    public string? Gender { get; set; }

    /// <summary>
    /// Valid Values from GlobalConstants.MaritalStatuses
    /// </summary>
    [MaxLength(1)]
    public string? MaritalStatus { get; set; }

    [RegularExpression(@"^[A-Z\d]{0,}$", ErrorMessage = "'National ID' invalid format. Valid format input: Capital letter OR number.")]
    public string? NationalIdNum { get; set; }

    public DateTime? NationalIDExpiryDate { get; set; }

    [RegularExpression(@"^[A-Z\d]{0,}$", ErrorMessage = "'Passport No.' invalid format. Valid format input: Capital letter OR number.")]
    public string? PassportNo { get; set; }
    public DateTime? PassportExpiryDate { get; set; }

    [DataAnnot.Column("NatlCtyCode")]
	public string? NatlCtyCode { get; set; }
	//public string? NationalityCountryCode { get; set; }

	[DataType(DataType.EmailAddress)]
    [MaxLength(150)]
    public string? PersonalEmail { get; set; }

    [DataType(DataType.EmailAddress)]
    [MaxLength(150)]
    public string? WorkEmail { get; set; }

    [DataType(DataType.PhoneNumber)]
    [MaxLength(30)]
    public string? PhoneLine1 { get; set; }

    [DataType(DataType.PhoneNumber)]
    [MaxLength(30)]
    public string? PhoneLine2 { get; set; }

    [DataAnnot.Column("WorkAddrId")]
    public int? WorkAddrId { get; set; }

	[DataAnnot.Column("BirthAddrId")]
	public int? BirthAddrId { get; set; }
	//public int? BirthAddressId { get; set; }

	[DataAnnot.Column("ResAddrId")]
	public int? ResAddrId { get; set; }
	//public int? ResidentialAddressId { get; set; }

	[DataAnnot.Column("PostalAddrId")]
	public int? PostalAddrId { get; set; }
	//public int? PostalAddressId { get; set; }

	public string? ProfilePhotoImagePath { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed]
    [Description("ignore")]
    public Country? Nationality { get; set; }

    [Computed]
    [Description("ignore")]
    public CambodiaAddress? WorkAddress { get; set; }

    [Computed]
    [Description("ignore")]
    public CambodiaAddress? BirthAddress { get; set; }

    [Computed]
    [Description("ignore")]
    public CambodiaAddress? ResidentialAddress { get; set; }

    [Computed]
    [Description("ignore")]
    public CambodiaAddress? PostalAddress { get; set; }

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public List<Contact> Contacts { get; set; }

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public List<ContactPhone> ContactPhones { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string AgeText => BirthDate != null ? BirthDate.GetAge(DeathDate).ToString() : "-";

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string BirthDateText => BirthDate == null ? "" : BirthDate.Value.ToString("dd-MMM-yyyy");

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string NationalIDExpiryDateText => NationalIDExpiryDate == null ? "" : NationalIDExpiryDate.Value.ToString("dd-MMM-yyyy");

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string PassportExpiryDateText => PassportExpiryDate == null ? "" : PassportExpiryDate.Value.ToString("dd-MMM-yyyy");

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string GenderText => Genders.GetDisplayText(Gender!);

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string MaritalStatusText => MaritalStatuses.GetDisplayText(MaritalStatus!);

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string NationalityText => Nationality != null ? Nationality.ObjectName.NonNullValue("-") : "-";

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string FullNameEnText => ((GivenName ?? "") + " " + (string.IsNullOrEmpty(MiddleName) ? "" : MiddleName + " ") + (Surname ?? "")).Trim();

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string FullNameKhText => ((SurnameKh ?? "") + " " + (GivenNameKh ?? "")).Trim();
    #endregion

    public Person() : base()
    {
        Contacts = [];
        ContactPhones = [];
    }
}