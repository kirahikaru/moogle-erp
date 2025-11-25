using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("UnitOfMeasure"), DisplayName("Unit of Measure")]
public class UnitOfMeasure : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(UnitOfMeasure).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"unit_of_measure";

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

    [Required(AllowEmptyStrings = false, ErrorMessage = "'NAME' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

    /// <summary>
    /// Valid Values => GlobalConstants.UnitOfMeasureTypes
    /// </summary>
    public string? UnitOfMeasureType { get; set; }

    [Required]
    public string? UnitSymbol { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false), ReadOnly(true)]
    public string NameWithSymbol => $"{ObjectName ?? ""} ({UnitSymbol ?? ""})";
	#endregion
}