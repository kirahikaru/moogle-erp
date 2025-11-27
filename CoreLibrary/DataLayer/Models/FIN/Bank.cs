using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;

namespace DataLayer.Models.FIN;

/// <summary>
/// User generically here for both Banks and MFI
/// </summary>
[Table("[fin].[Bank]")]
public class Bank : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.FINANCE;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Bank).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "bank";

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

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Bank Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
    public string? RegisteredName { get; set; }
    [StringUnicode(true)]
    public string? ObjectNameKh { get; set; }
    public string? DisplayName { get; set; }
    /// <summary>
    /// Valid Values > Global Constants FIN > BankTypes
    /// </summary>
    [Required(ErrorMessage = "'Bank Type' is required.")]
    public string? BankType { get; set; }

    public string? AddressText { get; set; }
    public string? AddressTextKh { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false), ReadOnly(true)]
    public string? BankTypeText => BankTypes.GetDisplayText(BankType);
    #endregion

    #region *** LINKED OBJECT ***

    #endregion
}