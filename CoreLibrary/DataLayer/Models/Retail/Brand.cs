using DataLayer.GlobalConstant;
using DataLayer.Models.Procurement;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;

[Table("[rms].[Brand]"), DisplayName("Brand")]
public class Brand : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Brand).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "brand";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Code' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Code' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Name' is required.")]
    [RegularExpression(@"^[a-zA-Z\d\W\s]{0,}$", ErrorMessage = "'Name' invalid format.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

    #region *** DATABASE FIELDS ***
    public string? ObjectNameKh { get; set; }
	public string? CountryCode { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion
}