using DataLayer.GlobalConstant;

namespace DataLayer.Models.HMS;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[hms].[HealthcareFacility]"), DisplayName("Healthcare Facility")]
public class HealthcareFacility : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(HealthcareFacility).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "healthcare_facility";

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
    [RegularExpression(@"^[a-zA-Z\d_\W]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
    public string? ObjectNameKh { get; set; }
    /// <summary>
    /// Healthcare Facility Type
    /// {FK} > DropdownDataList.Id
    /// </summary>
    [Required(ErrorMessage = "'Facility Type' is required.")]
	[Description("DropdownDataDriven")]
	public int? FacilityTypeDdlId { get; set; }
	public string? FacilityTypeDdlCode { get; set; }

	public int? RegisteredAddressId { get; set; }
    public int? MainBranchAddressId { get; set; }
    public string? FacebookLink { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }

    public string? TelNo { get; set; }
	public string? PhoneLine1 { get; set; }
	public string? PhoneLine2 { get; set; }
    public string? Telegram { get; set; }
    public string? GoogleMapLink { get; set; }
    public string? LogoImagePath { get; set; }

	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public List<ContactPhone> Contacts { get; set; }

	[Computed, Write(false)]
	public CambodiaAddress? RegisteredAddress { get; set; }

	[Computed, Write(false)]
	public CambodiaAddress? MainBranchAddress { get; set; }

	[Computed, Write(false)]
	public DropdownDataList? FacilityType { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion

    public HealthcareFacility()
    {
        RegisteredAddress = new();
        MainBranchAddress = new();
        Contacts = [];
    }
}