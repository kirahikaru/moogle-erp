using DataLayer.GlobalConstant;

namespace DataLayer.Models.RMS;

[Table("[rms].[Manufacturer]")]
public class Manufacturer : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Manufacturer).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "manufacture";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public DateTime? OnboardDate { get; set; }
    public DateTime? OffboardDate { get; set; }

    /// <summary>
    /// Valid Values > GlobalConstants_RMS > ManufacturerStatuses
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "'Status' is required.")]
    public string? Status { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false), ReadOnly(true)]
	public Address? MainAddress { get; set; }
    #endregion

    #region *** DYANMIC PROPERTIES ***
    #endregion
}