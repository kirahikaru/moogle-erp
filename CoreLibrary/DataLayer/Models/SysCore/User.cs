using DataLayer.GlobalConstant;

namespace DataLayer.Models.SysCore;

[Table("User")]
public class User : AuditObject
{
	[Computed, Write(false), ReadOnly(false)]
    public new static string MsSqlTableName => $"{typeof(User).Name}";

	[Computed, Write(false), ReadOnly(false)]
	public new static string PgTableName => typeof(User).Name.ToLower();

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'User ID' is required.")]
	public string? UserId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'User Name' is required.")]
    public string? UserName { get; set; }

    public string? EmployeeId { get; set; }

    [DataType(DataType.EmailAddress)]
    public string? PrimaryEmail { get; set; }

	[DataType(DataType.PhoneNumber)]
	public string? PrimaryPhoneNo { get; set; }

	[DataType(DataType.EmailAddress)]
	public string? SecondaryEmail { get; set; }

	[DataType(DataType.PhoneNumber)]
	public string? SecondaryPhoneNo { get; set; }
    public DateTime? ActivatedDateTime { get; set; }
    /// <summary>
    /// GlobalConstants.SystemCode.UserStatuses
    /// </summary>
    [Required(AllowEmptyStrings =false, ErrorMessage ="'Status' is required.")]
    public string? Status { get; set; }

	/// <summary>
	/// GlobalConstants.SystemCode.UserTypes
	/// </summary>
	[Required(AllowEmptyStrings = false, ErrorMessage = "'User Type' is required.")]
	public string? UserType { get; set; }
    public bool IsEnabled { get; set; }


    public int PrivacyAccessLevel { get; set; }
    public DateTime? TerminatedDateTime { get; set; }
    public int? ReportToUserId { get; set; }
    public string? HierarchyPath { get; set; }
    public string? Remark { get; set; }
    public int? OrgStructId { get; set; }
    public int? OrgId { get; set; }
    public int? OrgBranchId { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Organization? Organization { get; set; }

	[Computed, Write(false)]
	public OrgStruct? OrgStruct { get; set; }

	[Computed, Write(false)]
	public User? ReportToUser { get; set; }

	[Computed, Write(false)]
	public UserAccount? Account { get; set; }

    [Computed, Write(false)]
    public List<Permission> Permissions { get; set; }

	[Computed, Write(false)]
    public List<UserRole> Roles { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false), ReadOnly(true)]
	public string UserNameWithUserId
    {
        get
        {
            StringBuilder sb = new();

            if (!string.IsNullOrEmpty(UserName))
                sb.Append(UserName + " ");

            if (!string.IsNullOrEmpty(UserId))
                sb.Append($" ({UserId})");

            return sb.ToString();
        }
    }
    #endregion

    public User() : base()
    {
        UserType = UserTypes.GENERAL;
        Status = UserStatuses.ACTIVE;
        Permissions = [];
        Roles = [];
    }
}