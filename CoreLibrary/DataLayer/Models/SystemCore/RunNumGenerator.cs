using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("RunNumGenerator")]
public class RunNumGenerator : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(RunNumGenerator).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"run_num_generator";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ObjectClassName { get; set; }
    public string? DisplayFormat { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }

    /// <summary>
    /// ValidValues > Global Constants > System Intervals
    /// </summary>
    public string? ResetInterval { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}