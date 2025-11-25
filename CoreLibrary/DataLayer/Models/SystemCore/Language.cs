using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

/// <summary>
/// Source: https://lonewolfonline.net/list-net-culture-country-codes/
/// </summary>
[Table("Language")]
public class Language : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(Language).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"language";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? LanguageCode2 { get; set; }
    public string? LanguageCode3 { get; set; }
    public string? CountryName { get; set; }
    public string? CountryCode2 { get; set; }
    public string? CountryCode3 { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion
}