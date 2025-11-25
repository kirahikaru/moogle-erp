using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Finance;

[Table("[fin].[TaxRate]")]
public class TaxRate : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.FINANCE;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(TaxRate).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "tax_rate";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? TaxId { get; set; }
    public decimal? RatePercentage { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinApplicableAmount { get; set; }
    public decimal? MaxApplicableAmount { get; set; }
    public bool IsForForeigner { get; set; }
    #endregion

    #region *** LINKED OBJECT ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}