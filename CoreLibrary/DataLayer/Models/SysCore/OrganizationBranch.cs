namespace DataLayer.Models.SysCore;

[Table("OrganizationBranch")]
public class OrganizationBranch : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => "OrgBranch";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"org_branch";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ObjectNameKh { get; set; }
    

    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
    public string? FaxNumber { get; set; }
    public string? FacebookLink { get; set; }
    public string? InstagramLink { get; set; }
    public string? LinkedInLink { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? VillageCode { get; set; }
    public string? CommuneCode { get; set; }
    public string? DistrictCode { get; set; }
    public string? ProvinceCode { get; set; }

    [DataType(DataType.PhoneNumber)]
    public string? HotLine { get; set; }

    public int? OrganizationId { get; set; }
    public int? AddressId { get; set; }
    public int? CambodiaAddressId { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Organization? Organization { get; set; }

	[Computed, Write(false)]
	public Address? Address { get; set; }

	[Computed, Write(false)]
	public CambodiaAddress? CambodiaAddress { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}