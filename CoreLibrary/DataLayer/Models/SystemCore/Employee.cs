using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Use in LMS
/// </remarks>
[Table("Employee")]
public class Employee : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(Employee).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"employee";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Employee ID' is required.")]
	[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Employee ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	[MaxLength(80)]
	public new string? ObjectCode { get; set; }

	[MaxLength(150), StringUnicode(true)]
	public string? ObjectNameKh { get; set; }

	/// <summary>
	/// a.k.a LastName
	/// </summary>
	[MaxLength(150)]
	public string? Surname { get; set; }

	[MaxLength(150), StringUnicode(true)]
	public string? SurnameKh { get; set; }
	public string? MiddleName { get; set; }
	public string? JobTitle { get; set; }
    public int? JobPositionId { get; set; }
    public string? JobPositionName { get; set; }

    /// <summary>
    /// Data List > Global Constants > SystemCore > EmployeeContractTypes  
    /// </summary>
    [Required(ErrorMessage = "'Contract Type' is required.")]
    public string? ContractType { get; set; }

	/// <summary>
	/// Data List > Global Constants > SystemCore > EmployeeTimeTypes  
	/// </summary>
	[Required(ErrorMessage = "'Time Type' is required.")]
	public string? TimeType { get; set; }

	/// <summary>
	/// a.k.a FirstName
	/// </summary>
	[MaxLength(150)]
	public string? GivenName { get; set; }

	[MaxLength(150), StringUnicode(true)]
	public string? GivenNameKh { get; set; }

	[Required(ErrorMessage = "'Date of Birth' is required.")]
	public DateTime? BirthDate { get; set; }
	/// <summary>
	/// Data List > GlobalConstants > SystemCore > Genders
	/// </summary>
	[Required]
	[MaxLength(1)]
	public string? Gender { get; set; }

	/// <summary>
	/// Data List > GlobalConstants > SystemCore > MaritalStatuses
	/// </summary>
	[MaxLength(1)]
	public string? MaritalStatus { get; set; }

	[RegularExpression(@"^[A-Z\d]{0,}$", ErrorMessage = "'Nationall ID' invalid format. Valid format input: Capital letter OR number.")]
	public string? NationalIdNum { get; set; }

	public DateTime? NationalIDExpiryDate { get; set; }

	[RegularExpression(@"^[A-Z\d]{0,}$", ErrorMessage = "'Nationall ID' invalid format. Valid format input: Capital letter OR number.")]
	public string? PassportNo { get; set; }
	public DateTime? PassportExpiryDate { get; set; }

	/// <summary>
	/// Nationality Country
	/// </summary>
	[Required]
	public string? NatlCtyCode { get; set; }

	[DataType(DataType.EmailAddress, ErrorMessage = "'Personal Email' invalid email format.")]
	[MaxLength(150)]
	public string? PersonalEmail { get; set; }

    [DataType(DataType.EmailAddress, ErrorMessage = "'Work Email' invalid email format.")]
    [MaxLength(150)]
	public string? WorkEmail { get; set; }

	[DataType(DataType.PhoneNumber)]
	[MaxLength(30)]
	public string? PhoneLine1 { get; set; }

	[DataType(DataType.PhoneNumber)]
	[MaxLength(30)]
	public string? PhoneLine2 { get; set; }	

	[Required(ErrorMessage = "'Joined Date' is required.")]
    public DateTime? JoinedDate { get; set; }
    public DateTime? LastDate { get; set; }
	[Required(ErrorMessage = "'Employee Status' is required.")]
    public string? Status { get; set; }
    public int? UserId { get; set; }
    public string? UserUserId { get; set; }

	/// <summary>
	/// Organizational Structure
	/// </summary>
	public int? OrgStructId { get; set; }

	/// <summary>
	/// Residential Address
	/// </summary>
	public int? ResAddrId { get; set; }

	/// <summary>
	/// Correspondence Address
	/// </summary>
	public int? CorrespAddrId { get; set; }
	public string? ProfilePhotoImagePath { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed]
	[Description("ignore")]
    public User? User { get; set; }

	[Computed]
	[Description("ignore")]
	public CambodiaAddress? ResidentialAddress { get; set; }

	[Computed]
	[Description("ignore")]
	public CambodiaAddress? CorrespondentAddress { get; set; }

	[Computed]
	[Description("ignore")]
	public OrgStruct? Team { get; set; }

	[Computed]
	[Description("ignore")]
	public OrgStruct? Function { get; set; }

	[Computed]
	[Description("ignore")]
	public OrgStruct? Department { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed]
    [Description("ignore")]
	public Country? Nationality { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string FullNameEnText => ((GivenName ?? "") + " " + (string.IsNullOrEmpty(MiddleName) ? "" : MiddleName + " ") + (Surname ?? "")).Trim();

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string FullNameKhText => ((SurnameKh ?? "") + " " + (GivenNameKh ?? "")).Trim();

    [Computed]
	[Description("ignore"), ReadOnly(true)]
	public string AgeText => BirthDate != null ? BirthDate.GetAge().ToString() : "-";

	[Computed]
	[Description("ignore"), ReadOnly(true)]
	public string GenderText => Genders.GetDisplayText(Gender);

	[Computed]
	[Description("ignore"), ReadOnly(true)]
	public string MaritalStatusText => MaritalStatuses.GetDisplayText(MaritalStatus);

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
	public string NationalityText => Nationality != null ? Nationality.ObjectName.NonNullValue("-") : "-";

	[Computed]
	[Description("ignore"), ReadOnly(true)]
	public string StatusText => EmployeeStatuses.GetDisplayText(Status);
	#endregion

	public Employee() : base()
    {
		Status = EmployeeStatuses.DRAFT;
    }
}