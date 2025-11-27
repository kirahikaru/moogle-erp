using DataLayer.GlobalConstant;

namespace DataLayer.Models.RMS;

[Table("[rms].[CustPurchaseInvItem]"), DisplayName("Customer Purchase Invoice Item")]
public class CustPurchaseInvItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(CustPurchaseInvItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "cust_purchase_inv_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ObjectNameKh { get; set; }
    public int? ItemId { get; set; }
    public int? SequenceNo { get; set; }
    public string? Barcode { get; set; }
    public int? CustPurchaseInvId { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? UnitPriceKhr { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? TotalAmountKhr { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountAmountKhr { get; set; }
    public decimal? TotalPayableAmount { get; set; }
    public decimal? TotalPayableAmountKhr { get; set; }

    public DateTime? PaidDateTime { get; set; }
    public string? PaymentRefNo { get; set; }
    public string? PaymentRemark { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed]
    [Description("ignore")]
    public Item? Item { get; set; }

    [Computed]
    [Description("ignore")]
    public CustPurchaseInvoice? Invoice { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}