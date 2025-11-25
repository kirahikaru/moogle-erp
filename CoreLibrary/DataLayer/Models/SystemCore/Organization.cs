using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("Org")]
public class Organization : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => "Org";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"org";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Code' is required.")]
	[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Code' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	[MaxLength(80)]
	public new string? ObjectCode { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Name' is required.")]
	[RegularExpression(@"^[a-zA-Z\d\s\W]{0,}$", ErrorMessage = "'Name' is invalid format.")]
	[MaxLength(255)]
	public new string? ObjectName { get; set; }
	public string? ObjectNameKh { get; set; }
    public string? IndustryCode { get; set; }
    public string? HotLine { get; set; }

    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
    public string? FaxNumber { get; set; }
    [DataType(DataType.Url)]
    public string? Website { get; set; }
    public string? FacebookLink { get; set; }
    public string? InstagramLink { get; set; }
    public string? LinkedInLink { get; set; }
    public string? YouTubeLink { get; set; }
    public int? LocationId { get; set; }
    public bool IsEnabled { get; set; }
    public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Location? Location { get; set; }

    [Computed, Write(false)]
    public List<OrganizationBranch> Branches { get; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion

    public Organization() : base()
    {
        Branches = [];
    }
}