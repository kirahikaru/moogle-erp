using DataLayer.GlobalConstant;
using Dapper.Contrib.Extensions;

namespace DataLayer.Models.FIN;

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
	public int? FromCurrencyRatio { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'To Currency' is required.")]
	public string? ToCurrencyCode { get; set; }

	[Required(ErrorMessage = "'To Currency Ratio' is required.")]
	[Range(0, 999999999, ErrorMessage = "'To Currency Ratio' must be positive wholenumber.")]
	public int? ToCurrencyRatio { get; set; }
    [Required(ErrorMessage = "'Start Date' is required.")]
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
	/// <summary>
	/// Exchange Type : Default, Buy, Sell, Mid
	/// Valid Values: GlobalConstants.FIN.ExchangeTypes
	/// </summary>
	[Required(ErrorMessage = "'Exchange Type' is required.")]
	public string? ExchgType { get; set; }
	public bool IsCurrent { get; set; }
	public string? Source { get; set; }
	public string? Note { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false)]
    public string ExchangeRateText => $"{FromCurrencyCode.NonNullValue("-")} {FromCurrencyRatio:#,##0} = {ToCurrencyCode.NonNullValue("-")} {ToCurrencyRatio:#,##0}";
    #endregion

    public decimal ComputeTo(decimal fromCurrValue)
    {
        return (fromCurrValue * ToCurrencyRatio!.Value)/FromCurrencyRatio!.Value;
    }

    public decimal ComputeFrom(decimal toCurrValue)
    {
        return (toCurrValue * FromCurrencyRatio!.Value) / ToCurrencyRatio!.Value;
    }

    public ExchangeRate()
    {
        IsCurrent = true;
        ExchgType = ExchangeTypes.DEFAULT;
    }
}