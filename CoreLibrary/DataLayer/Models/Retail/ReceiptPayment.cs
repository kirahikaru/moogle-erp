using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.Finance;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;

[Table("[rms].[ReceiptPayment]"), DisplayName("Payment")]
public class ReceiptPayment : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ReceiptPayment).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "receipt_payment";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	/// <summary>
	/// Receipt.ObjectCode
	/// </summary>
	public int? ReceiptId { get; set; }

    /// <summary>
    /// Valid Values > GlobalConstants.RMS.PaymentOptions
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "'Payment Option' is required.")]
    public string? PaymentOption { get; set; }
    public int? BankId { get; set; }

	[Required(ErrorMessage = "'Amount Due in USD is required.'")]
    public decimal? DueAmountUsd { get; set; }

    [Required(ErrorMessage = "'Amount Due in KHR is required.'")]
    public decimal? DueAmountKhr { get; set; }

	public string? AccountNo { get; set; }
	public string? AccountName { get; set; }

	[DataType(DataType.CreditCard)]
	public string? CCNo { get; set; }
	public string? CCName { get; set; }
	public int? CCExpiryMonth { get; set; }
	public int? CCExpiryYear { get; set; }

	/// <summary>
	/// if CurrencyCode='USD'
	/// AmountPaid = CashPaidUsd + CashPaidKhr/{ExchangeRate}
	/// 
	/// if CurrencyCode='KHR'
	/// AmountPaid = CashPaidUsd*{ExchangeRate} + CashPaidKhr
	/// </summary>
	[Precision(18, 2)]
	public decimal? AmountPaid { get; set; }

	/// <summary>
	/// User input
	/// </summary>
	public decimal? CashPaidUsd { get; set; }

	/// <summary>
	/// User input
	/// </summary>
	public int? CashPaidKhr { get; set; }

	/// <summary>
	/// ChangeAmount = AmountPaid - TotalNetPayableAmount
	/// </summary>
	[Precision(18, 2)]
	public decimal? ChangeAmount { get; set; }

	/// <summary>
	/// If CurrencyCode='USD' => CashChangeUsd=ChangeAmount
	/// If CurrencyCode='KHR' => CashChangeUsd=ChangeAmount/{ExchangeRate}
	/// </summary>
	public decimal? CashChangeUsd { get; set; }

	/// <summary>
	/// If CurrencyCode='USD' => CashChangeKhr=ChangeAmount*{ExchangeRate}
	/// If CurrencyCode='KHR' => CashChangeUsd=ChangeAmount
	/// </summary>
	public int? CashChangeKhr { get; set; }
	#endregion

	#region *** DYANMIC PROPERTIES ***
	[Computed, Write(false)]
	public string DueAmountUsdText => DueAmountUsd != null ? $"${DueAmountUsd!.Value:#,##0.00}" : "-";

	[Computed, Write(false)]
	public string DueAmountKhrText => DueAmountKhr != null ? $"៛{DueAmountKhr!.Value:#,##0}" : "-";
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
	public Bank? Bank { get; set; }
	#endregion

	public ReceiptPayment() : base()
    {
        
    }

    public ReceiptPayment(int receiptId, decimal dueAmountUsd, decimal dueAmountKhr) : base()
    {
        ReceiptId = receiptId;
        DueAmountUsd = dueAmountUsd;
        DueAmountKhr = dueAmountKhr;
	}

	public void ClearValues()
	{
		ChangeAmount = null;
		CashChangeKhr = null;
		CashChangeUsd = null;
		CashPaidKhr = null;
		CashPaidUsd = null;
		CCExpiryMonth = null;
		CCExpiryYear = null;
		CCName = null;
		CCNo = null;
		BankId = null;
		Bank = null;
		AccountName = null;
		AccountNo = null;
	}
}