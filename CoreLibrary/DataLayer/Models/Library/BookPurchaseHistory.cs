using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.HomeInventory;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Library;
[Table("[lib].[BookPurchaseHistory]"), DisplayName("Book Purchase History")]
public class BookPurchaseHistory : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.LIBRARY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(BookPurchaseHistory).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "book_purchase_history";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? BookId { get; set; }
    public int? UserBookId { get; set; }
    public DateTime? PurchasedDate { get; set; }

    public int? MerchantId { get; set; }

    [MaxLength(150)]
    public string? MerchantCode { get; set; }

    [Precision(10, 2)]
    public decimal? UnitPrice { get; set; }

    [Precision(10, 2)]
    public int? Quantity { get; set; }

    [Precision(18, 2)]
    public decimal? DiscountAmount { get; set; }

    [Precision(18, 2)]
    public decimal? PurchasedPrice { get; set; }

    public string? BookFormat { get; set; }
    public bool IsEBook { get; set; }

    [MaxLength(255)]
    public string? Remark { get; set; }

    public bool IsPlasticWrapped { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Book? Book { get; set; }

	[Computed, Write(false)]
	public UserBook? UserBook { get; set; }

	[Computed, Write(false)]
	public Merchant? Merchant { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string UnitPriceText => UnitPrice.HasValue ? $"$ {UnitPrice!.Value:#,##0.00}" : "$ -";

	[Computed, Write(false), ReadOnly(true)]
	public string QuantityText => Quantity.HasValue ? $"$ {Quantity!.Value:#,##0}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string PurchasedPriceText => PurchasedPrice.HasValue ? $"$ {PurchasedPrice!.Value:#,##0.00}" : "$ -";

	[Computed, Write(false), ReadOnly(true)]
	public string DiscountAmountText => DiscountAmount.HasValue ? $"$ {DiscountAmount!.Value:#,##0.00}" : "$ -";
	#endregion
}