namespace DataLayer.Models.SysCore;

[Table("Country")]
public class Country : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(Country).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"country";

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

    [RegularExpression(@"^[a-zA-Z\d\s\W]{0,}$", ErrorMessage = "'Name' invalid format.")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "'Name' is required.")]
    [MaxLength(255)]
    public string? NameEn { get; set; }
    public string? NameKh { get; set; }
    public string? FullName { get; set; }

    [RegularExpression(@"^[A-Z]{0,}$", ErrorMessage = "'Code Alpha 3' invalid format.")]
    [MaxLength(3)]
    public string? CodeAlpha3 { get; set; }

    [RegularExpression(@"^[A-Z]{0,}$", ErrorMessage = "'Code Alpha 2' invalid format.")]
    [MaxLength(2)]
    public string? CodeAlpha2 { get; set; }

    public string? UNCode { get; set; }
    public string? Nationality { get; set; }
    public string? Language { get; set; }

    public string? FlagIconPath { get; set; }
    public string? CountryCallingCode { get; set; }
    public bool IsCallCodeSupported { get; set; }
    public bool IsCountrySupported { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}