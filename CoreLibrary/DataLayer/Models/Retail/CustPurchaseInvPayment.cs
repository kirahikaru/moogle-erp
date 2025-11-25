using DataLayer.GlobalConstant;
using DataLayer.Models.Finance;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;

[Table("[rms].[CustomerPurchaseInvoicePayment]"), DisplayName("Customer Purchase Invoice Payment")]
public class CustPurchaseInvPayment : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "CustPurchaseInvPayment";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "cust_purchase_inv_payment";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Item Code' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    #region *** DATABASE FIELDS ***
    public int? CustomerPurchaseInvoiceId { get; set; }
    /// <summary>
    /// Valid Values > GlobalConstants_RMS > RetailPaymentMethods
    /// </summary>
    [Required(ErrorMessage = "'Payment Method' is required")]
    public string? PaymentMethod { get; set; }
    public string? PaymentRefNum { get; set; }
    public int? BankId { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? ChequeNo { get; set; }
    public string? ChequeDate { get; set; }

    /// <summary>
    /// Credit Card Number
    /// </summary>
    public string? CcNo { get; set; }
    /// <summary>
    /// Credit Card Expiry Date
    /// </summary>
    public DateTime? CcExpiryDate { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? PaymentAmount { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public CustPurchaseInvoice? PurchaseInvoice { get; set; }

	[Computed, Write(false)]
	public Bank? Bank { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}