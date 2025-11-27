using DataLayer.GlobalConstant;

namespace DataLayer.Models.LIB;

[Table("[lib].[UserBook]"), DisplayName("My Book")]
public class UserBook : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.LIBRARY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(UserBook).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "user_book";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? UserId { get; set; }
    public int? BookId { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? ReadStartDate { get; set; }
    public DateTime? ReadEndDate { get; set; }
    public int Rating { get; set; }
    public bool? IsGift { get; set; }
    public string? GiftFrom { get; set; }
    public DateTime? PurchaseDate { get; set; }

    [Range(0, double.MaxValue)]
    [DataType(DataType.Currency)]
    public decimal? PurchasePrice { get; set; }
    public string? PurchaseLocation { get; set; }
    public bool? IsEBookAvailable { get; set; }
    public string? EBookFileLocation { get; set; }

    /// <summary>
    /// Valid Values > Global Constants LIB > UserBookOwnershipStatuses
    /// </summary>
    public string? OwnershipStatus { get; set; }
    public string? Remark { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
	public User? User { get; set; }

	[Computed, Write(false)]
	public Book? Book { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string PurchasedPriceText => PurchasePrice.HasValue ? PurchasePrice.Value.ToCurrencyText(true, "$") : "$ -";
    #endregion

    public UserBook() : base()
    {
        IsRead = false;
        IsGift = false;
        IsEBookAvailable = false;
        Rating = 0;
        OwnershipStatus = UserBookOwnershipStatuses.AVAILABLE; 
    }
}