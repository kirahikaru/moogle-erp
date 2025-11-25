using DataLayer.GlobalConstant.Pru;
using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.Finance;

[Table("[dbo].[Vendor]"), DisplayName("Supplier / Vendor")]
public class Vendor : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Vendor).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "vendor";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Vendor ID' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Vendor ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Vendor Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
    public string? ScopeOrServices { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Contract Name' is required.")]
	public string? ContractName { get; set; }
    public string? AddressText { get; set; }
    public string? Contacts { get; set; }
	public string? TaxRegNo { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'LBU' is required.")]
	public string? LBU { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Status' is required.")]
    public string? Status { get; set; }

    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

	[DataType(DataType.Date)]
	public DateTime? EndDate { get; set; }
	public string? Remark { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false)]
	public string ObjectNameAndCode => $"{ObjectName.NonNullValue("-")} ({ObjectCode.NonNullValue("-")})";

    [Computed, Write(false)]
    public string StatusText => VendorStatuses.GetDisplayText(Status);
	#endregion

	public Vendor() : base()
    {
        
    }
}