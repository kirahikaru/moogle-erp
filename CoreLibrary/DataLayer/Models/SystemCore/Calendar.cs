using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;
/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[dbo].[Calendar]")]
public class Calendar : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(Calendar).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"calendar";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(ErrorMessage = "'Calendar Date' is required.")]
    public DateTime? CalendarDate { get; set; }
    public int? DayOfMonth { get; set; }
    public string? DayOfWeekName { get; set; }
    public int? DayOfWeek { get; set; }
    public int? WeekOfYear { get; set; }
    public int? ISOWeekOfYear { get; set; }
    public int? MonthOfYear { get; set; }
    public string? MonthName { get; set; }
    public int? Quarter { get; set; }
    public int? DayOfYear { get; set; }
    public int? Year { get; set; }
    public string? SpecialOccationDesc { get; set; }
    public string? Remark { get; set; }
    public bool IsWorkingDay { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    
    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion
}