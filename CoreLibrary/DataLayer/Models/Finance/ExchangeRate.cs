using DataLayer.AuxComponents.Extensions;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Finance;

[Table("[fin].[ExchangeRate]"), DisplayName("Exchange Rate")]
public class ExchangeRate : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.FINANCE;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ExchangeRate).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "exchange_rate";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'From Currency' is required.")]
	public string? FromCurrencyCode { get; set; }
    
    [Required(ErrorMessage = "'From Currency Ratio' is required.")]
    [Range(0, 999999999, ErrorMessage = "'From Currency Ratio' must be positive wholenumber.")]
	public int? FromCurrencyRatioValue { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'To Currency' is required.")]
	public string? ToCurrencyCode { get; set; }

	[Required(ErrorMessage = "'To Currency Ratio' is required.")]
	[Range(0, 999999999, ErrorMessage = "'To Currency Ratio' must be positive wholenumber.")]
	public int? ToCurrencyRatioValue { get; set; }
    [Required(ErrorMessage = "'Start Date' is required.")]
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false)]
    public string ExchangeRateText => $"{FromCurrencyCode.NonNullValue("-")} {FromCurrencyRatioValue:#,##0} = {ToCurrencyCode.NonNullValue("-")} {ToCurrencyRatioValue:#,##0}";
    #endregion

    public decimal ComputeTo(decimal fromCurrValue)
    {
        return (fromCurrValue * ToCurrencyRatioValue!.Value)/FromCurrencyRatioValue!.Value;
    }

    public decimal ComputeFrom(decimal toCurrValue)
    {
        return (toCurrValue * FromCurrencyRatioValue!.Value) / ToCurrencyRatioValue!.Value;
    }

    public ExchangeRate()
    {
        IsCurrent = true;
    }
}