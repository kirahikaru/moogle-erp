using DataLayer.GlobalConstant;
using DataAnnot = System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Models.HMS;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[hms].[Patient]")]
public class Patient : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Patient).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "patient";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ObjectNameKh { get; set; }
	public string? Surname { get; set; }
    public string? GivenName { get; set; }
    public string? SurnameKh { get; set; }
    public string? GivenNameKh { get; set; }

	[Required(ErrorMessage = "'Gender' is required.")]
	public DateTime? BirthDate { get; set; }
    /// <summary>
    /// Data List > GlobalConstants > SystemCore > Genders
    /// </summary>
    [Required(ErrorMessage = "'Gender' is required.")]
    [MaxLength(1)]
    public string? Gender { get; set; }

    /// <summary>
    /// Data List > GlobalConstants > SystemCore > MaritalStatuses
    /// </summary>
    [MaxLength(1)]
    public string? MaritalStatus { get; set; }

    [RegularExpression(@"^[A-Z\d]{0,}$", ErrorMessage = "'Nationall ID' invalid format. Valid format input: Capital letter OR number.")]
    public string? NationalIdNum { get; set; }

    [DataAnnot.Column("NatlIDEpiryDate")]
    public DateTime? NatlIDExpiryDate { get; set; }

    [RegularExpression(@"^[A-Z\d]{0,}$", ErrorMessage = "'Nationall ID' invalid format. Valid format input: Capital letter OR number.")]
    public string? PassportNo { get; set; }
    public DateTime? PassportExpiryDate { get; set; }

	[DisplayName("Nationality Country ID")]
	[DataAnnot.Column("NatlCtyCode")]
	[Required]
	public string? NatlCtyCode { get; set; }
	//public string? NationalityCountryCode { get; set; }

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

    [DataAnnot.Column("RegDateTime")]
    public DateTime? RegDateTime { get; set; }
    public int? HealthcareFacilityId { get; set; }
    public int? CustomerId { get; set; }
	public string? CustomerCode { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
    public Customer? Customer { get; set; }

	[Computed, Write(false)]
	public HealthcareFacility? HealthcareFacility { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, ReadOnly(true)]
    public string FullNameText => $"{GivenName ?? ""} {Surname ?? ""}".Trim();

	[Computed, ReadOnly(true)]
	public string FullNameKhText => $"{Surname ?? ""} {GivenName ?? ""}".Trim();
	#endregion
}