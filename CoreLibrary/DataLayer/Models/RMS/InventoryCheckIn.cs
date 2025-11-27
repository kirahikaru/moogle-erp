using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;

namespace DataLayer.Models.RMS;

[DisplayName("Inventory Check-In")]
[Table("[rms].[InventoryCheckIn]")]
public class InventoryCheckIn : WorkflowEnabledObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(InventoryCheckIn).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "inventory_check_in";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public DateTime? CheckInDateTime { get; set; }
    [Required(ErrorMessage = "'Currency' is required.")]
    public string? CurrencyCode { get; set; }

    [Precision(18, 2)]
    public decimal? TotalAmount { get; set; }

    [Precision(18, 2)]
    public decimal? TotalItemDiscountAmount { get; set; }

    [Range(0.00, double.MaxValue, ErrorMessage = "'Discount Amount' must be greater than 0.")]
    public decimal? DiscountAmount { get; set; }
    public decimal? TotalPayableAmount { get; set; }

    [Required(ErrorMessage = "'Supplier' is required.")]
    public int? SupplierId { get; set; }
    public string? SupplierInvoiceRefNum { get; set; }
    public int? InitiatorUserId { get; set; }
    public int? ApprovedUserId { get; set; }

    [MaxLength(255)]
    public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Currency? Currency { get; set; }

	[Computed, Write(false)]
	public Supplier? Supplier { get; set; }

	[Computed, Write(false)]
	public List<InventoryCheckInItem> Items { get; set; }

	[Computed, Write(false)]
	public User? InitiatorUser { get; set; }

	[Computed, Write(false)]
	public User? ApprovedUser { get; set; }

	[Computed, Write(false)]
	public User? AssignedUser { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false), ReadOnly(true)]
	public string WorkflowStatusText => WorkflowStatuses.GetDisplayText(WorkflowStatus);

    public decimal ComputedQuantity
    {
        get
        {
            decimal total = 0;

            foreach (InventoryCheckInItem item in Items)
                if (!item.IsDeleted)
                    total += (item.Quantity ?? 0);

            return total;
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalAmount
    {
        get
        {
            decimal totalAmount = 0;

            foreach (InventoryCheckInItem item in Items)
                if (!item.IsDeleted)
                    totalAmount += (item.Amount ?? 0);

            return totalAmount;
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalItemDiscountAmount
    {
        get
        {
            decimal totalAmount = 0;

            foreach (InventoryCheckInItem item in Items)
                if (!item.IsDeleted)
                    totalAmount += (item.DiscountAmount ?? 0);

            return totalAmount;
        }
    }

    [Computed, Write(false), ReadOnly(true)]
	public decimal ComputedTotalPayableAmount
    {
        get
        {
            decimal totalAmount = 0;

            foreach (InventoryCheckInItem item in Items)
                if (!item.IsDeleted)
                    totalAmount += (item.PayableAmount ?? 0);

            if (DiscountAmount.HasValue)
                totalAmount -= DiscountAmount!.Value;

            return totalAmount;
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string TotalDiscountAmountText => ((TotalItemDiscountAmount ?? 0) + (DiscountAmount ?? 0)).ToCurrencyText(CurrencyExtension.IsCurrencyHasDecimal(CurrencyCode), Currencies.GetSymbol(CurrencyCode));

	[Computed, Write(false), ReadOnly(true)]
	public string TotalPayableAmountText => TotalPayableAmount.ToCurrencyText(CurrencyExtension.IsCurrencyHasDecimal(CurrencyCode), Currencies.GetSymbol(CurrencyCode));

	[Computed, Write(false), ReadOnly(true)]
	public string TotalAmountText => TotalAmount.ToCurrencyText(CurrencyExtension.IsCurrencyHasDecimal(CurrencyCode), Currencies.GetSymbol(CurrencyCode));
    #endregion

    public InventoryCheckIn() :base()
    {
        Items = [];
        CurrencyCode = Currencies.US_USD;
    }
}