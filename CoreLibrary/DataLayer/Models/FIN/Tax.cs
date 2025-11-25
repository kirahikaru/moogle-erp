using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.FIN;

[Table("[fin].[Tax]")]
public class Tax : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.FINANCE;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Tax).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "tax";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? DisplayName { get; set; }
    public string? DisplayNameKh { get; set; }
    public bool IsEnabled { get; set; }
    #endregion

    #region *** LINKED OBJECT ***
    [Computed, Write(false)]
	public List<TaxRate> Rates { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    public decimal GetTaxAmount(decimal amount, bool isForeigner = false)
    {
        decimal val = 0;

        TaxRate? rate = this.Rates.Where(x =>
            (x.MinApplicableAmount == null || x.MinApplicableAmount!.Value <= amount)
            && (x.MaxApplicableAmount == null || x.MaxApplicableAmount!.Value >= amount)
            && x.IsForForeigner == isForeigner).SingleOrDefault();

        if (rate != null)
        {
            val = (amount * rate.RatePercentage!.Value) / 100;
        }

        return val;
    }

    public TaxRate? GetApplicableRate(decimal amount, bool isForeigner = false)
    {
        return Rates.Where(x =>
            (x.MinApplicableAmount == null || x.MinApplicableAmount!.Value <= amount)
            && (x.MaxApplicableAmount == null || x.MaxApplicableAmount!.Value >= amount)
            && x.IsForForeigner == isForeigner).SingleOrDefault();
    }
    #endregion

    public Tax()
    {
        Rates = [];
    }
}