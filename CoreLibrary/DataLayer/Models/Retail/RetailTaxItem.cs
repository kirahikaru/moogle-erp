using DataLayer.GlobalConstant;
using DataLayer.Models.Finance;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;

[Table("[rms].[RetailTaxItem]")]
public class RetailTaxItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(RetailTaxItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "retail_tax_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? LinkedObjectId { get; set; }
    public string? LinkedObjectType { get; set; }
    public int? TaxId { get; set; }
    public int? TaxRateId { get; set; }
    public string? TaxType { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public int? TaxAmountKhr { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Tax? Tax { get; set; }

	[Computed, Write(false)]
	public TaxRate? TaxRate { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false), ReadOnly(true)]
	public string TaxAmountText => TaxAmount.HasValue ? TaxAmount!.Value.ToCurrencyText(!CurrencyCode!.Is(Currencies.GetNoDecimalCurrencies()), Currencies.GetSymbol(CurrencyCode)) : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string DisplayName => ObjectName.NonNullValue("-") + (TaxPercentage.HasValue ? $" ({TaxPercentage.Value:#,##0}%)" : "");
    #endregion
}