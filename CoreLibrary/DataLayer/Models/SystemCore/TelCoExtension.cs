using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

/// <summary>
/// Align with Pan-Pru API Data Model
/// </summary>
[Table("TelCoExt")]
public class TelCoExtension : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => "TelCoExt";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"tel_co_ext";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? CallName { get; set; }
    public int? CountryId { get; set; }
    public string? CountryCode { get; set; }
    /// <summary>
    /// Telecommunication company extension number
    /// </summary>
    public string? TelCoExtNum { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}