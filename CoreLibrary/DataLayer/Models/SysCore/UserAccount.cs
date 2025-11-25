namespace DataLayer.Models.SysCore;

[Table("UserAccount"), DisplayName("User Account")]
public class UserAccount : AuditObject
{
	[Computed, Write(false), ReadOnly(false)]
	public new static string MsSqlTableName => $"{typeof(User).Name}";

	[Computed, Write(false), ReadOnly(false)]
	public new static string PgTableName => "user_account";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? UserName { get; set; }
    public int? UserId { get; set; }

    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    public bool IsEmailComfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsPhoneNumberConfirmed { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public bool IsFirstLogin { get; set; }
    public bool IsEnabled { get; set; }
    public bool EnforceFailLogInAttempt { get; set; }
    public int MaxFailedLogInAttempted { get; set; }
    public bool? IsPasswordExpired { get; set; }
    public DateTime? PasswordExpireDate { get; set; }
    public int? SessionDurationInSecond { get; set; }
    public bool IsConcurrentEnabled { get; set; }
    public int? MaxConcurrentAccessCount { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
	public User? User { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion

    public UserAccount()
    {
        IsEmailComfirmed = false;
        IsPhoneNumberConfirmed = false;
        IsTwoFactorEnabled = false;
        IsFirstLogin = true;
        IsEnabled = true;
        EnforceFailLogInAttempt = true;
        MaxFailedLogInAttempted = 5;
        IsPasswordExpired = false;
        IsConcurrentEnabled = true;
        MaxConcurrentAccessCount = 3;
    }

}