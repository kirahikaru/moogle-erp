using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

/// <summary>
/// Source: https://lonewolfonline.net/list-net-culture-country-codes/
/// </summary>
//[Table("[dbo].[SystemLanguageLocalization]")]
[Table("SysLangLocalization")]
public class SysLangLocalization : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(SysLangLocalization).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"sys_lang_localization";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? DisplayName { get; set; }
    public string? EnglishName { get; set; }
    public string? NativeName { get; set; }
    public string? ParentName { get; set; }
    public int? CountryId { get; set; }
    public string? CountryCode { get; set; }
    public bool IsSystemSupported { get; set; }
    public bool IsEnabled { get; set; }
    #endregion

    #region *** LINKED OBJECT ***
    [Computed]
    [Description("ignore")]
    public SysLangLocalization? Parent { get; set; }

    [Computed]
    [Description("ignore")]
    public Country? Country { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}