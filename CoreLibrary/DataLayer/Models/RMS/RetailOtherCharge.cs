using DataLayer.GlobalConstant;

namespace DataLayer.Models.RMS;

[Table("[rms].[RetailOtherCharge]")]
public class RetailOtherCharge : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(RetailOtherCharge).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "retail_other_charge";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? LinkedObjectId { get; set; }
    public string? LinkedObjectType { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ChargeAmount { get; set; }

    public int? ChargeAmountKhr { get; set; }
    public bool IsTaxable { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string ChargeAmountText 
    {
        get
        {
            return (ChargeAmount == null ? ChargeAmount!.Value.ToString("#,##0.00") : "-");
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string ChargeAmountKhrText
    {
        get
        {
            return (ChargeAmountKhr == null ? ChargeAmountKhr!.Value.ToString("#,##0") : "-");
        }
    }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Currency? Currency { get; set; }
    #endregion

    public RetailOtherCharge() : base()
    {
        IsTaxable = true;
    }
}