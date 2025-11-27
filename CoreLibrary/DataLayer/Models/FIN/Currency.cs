using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;

namespace DataLayer.Models.FIN;

[Table("[fin].[Currency]")]
public class Currency : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.FINANCE;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Currency).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "currency";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	/// <summary>
	/// Validation: 3 capital characters
	/// </summary>
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Currency Code' is required.")]
    [RegularExpression(@"^[a-zA-Z]{3,3}$", ErrorMessage = "'Currency Code' invalid format. Valid format 3 capital characters.")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Name' is required.")]
    [RegularExpression(@"^[a-zA-Z\d\s\W]{0,}$", ErrorMessage = "'Name' is invalid format.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

    [StringUnicode(true)]
    public string? CurrencySymbol { get; set; }
    public string? CountryCode { get; set; }
    #endregion

    #region *** LINKED OBJECT ***
    [Computed]
    [Description("ignore")]
    public Country? Country { get; set; }
    #endregion
}