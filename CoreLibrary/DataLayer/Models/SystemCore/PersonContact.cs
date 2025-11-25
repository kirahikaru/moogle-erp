using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("PersonContact")]
public class PersonContact : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(PersonContact).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"person_contact";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int PersonId { get; set; }
    public string? TypeCode { get; set; }
    public string? Label { get; set; }
    public string? Value { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Person? Person { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}