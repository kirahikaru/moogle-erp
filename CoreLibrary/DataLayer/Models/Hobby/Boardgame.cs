using DataLayer.GlobalConstant;
using DataLayer.Models.HomeInventory;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Hobby;

[Table("[home].[Boardgame]")]
public class Boardgame : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOME_INVENTORY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Boardgame).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "boardgame";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	/// <summary>
	/// 
	/// </summary>
	public string? VersionDesc { get; set; }
    public string? EditionDesc { get; set; }

    /// <summary>
    /// Original / Chinese Clone
    /// </summary>
    public string? Type { get; set; }

    [Range(1000, 9999, ErrorMessage = "'Release Year' invalid format. Only positive number year allowed.")]
    public int? ReleaseYear { get; set; }
    /// <summary>
    /// Valid Values > GlobalConstants.HIM.BoardgameStates
    /// </summary>
    [Required(ErrorMessage = "'Object State' is required.")]
    public string? ObjectState { get; set; }

    [RegularExpression(@"^[A-Z0-9]{0,}$", ErrorMessage = "'Barcode' invalid format.")]
    public string? Barcode { get; set; }

    [RegularExpression(@"^[A-Z0-9]{0,}$", ErrorMessage = "'ISBN' invalid format.")]
    public string? Isbn { get; set; }

    [DataType(DataType.Date)]
    public DateTime? PurchasedDate { get; set; }

    [DataType(DataType.Currency)]
    [Range(0.00, 99999999999.99, ErrorMessage = "'Release Year' invalid format. Only positive number year allowed.")]
    public decimal? PurchasedPrice { get; set; }
    public string? GameContents { get; set; }
    public string? GamePublisher { get; set; }
    public string? GameDesignerName { get; set; }

    [Range(0, 99999999, ErrorMessage = "'Age From' invalid format. Only positive number year allowed.")]
    public int? AgeFrom { get; set; }
    
    [Range(0, 99999999, ErrorMessage = "'Min. # of Players' invalid format. Only positive number year allowed.")]
    public int? MinPlayerNumber { get; set; }

    [Range(0, 99999999, ErrorMessage = "'Max. # of Players' invalid format. Only positive number year allowed.")]
    public int? MaxPlayerNumber { get; set; }
    /// <summary>
    /// Valid Values > GlobalConstants.UnitOfMeasuresTime
    /// </summary>
    public string? PlayDurationUnitCode { get; set; }

    [Range(0, 99999999, ErrorMessage = "'Max. # of Players' invalid format. Only positive number year allowed.")]
    public int? MinPlayDuration { get; set; }

    [Range(0, 99999999, ErrorMessage = "'Max. # of Players' invalid format. Only positive number year allowed.")]
    public int? MaxPlayDuration { get; set; }
    public int? MerchantId { get; set; }

    public string? SizeUnitCode { get; set; }
    public double? SizeLength { get; set; }
    public double? SizeHeight { get; set; }
    public double? SizeWidth { get; set; }

    public int? MainBoardGameId { get; set; }
    public bool IsExpansion { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? BoardGameGeekId { get; set; }
    public string? InfoUrl { get; set; }
    public string? MainImagePath { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Boardgame? MainBoardGame { get; set; }

	[Computed, Write(false)]
	public Merchant? Merchant { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? SizeUnit { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? DurationUnit { get; set; }

	[Computed, Write(false)]
	public List<BoardgameContentItem> ContentItems { get; set; }
    #endregion

    #region *** DYANMIC PROPERTIES ***
    [Computed, Write(false), ReadOnly(true)]
	public string PlayingAgeText
    {
        get
        {
            if (AgeFrom.HasValue)
            {
                return $"{AgeFrom.Value}+";
            }
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string PlayerLimitText
    {
        get
        {
            if (MinPlayerNumber.HasValue && MaxPlayerNumber.HasValue && MinPlayerNumber.Value != MaxPlayerNumber)
                return $"{MinPlayerNumber.Value} to {MaxPlayerNumber.Value} players";
            else if (MinPlayerNumber.HasValue)
                return $"{MinPlayerNumber.Value}+ players";
            else if (MaxPlayerNumber.HasValue)
                return $"<={MaxPlayerNumber.Value} players";
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string PlayDurationText
    {
        get
        {
            if (MinPlayDuration.HasValue && MaxPlayDuration.HasValue && MinPlayDuration.Value != MaxPlayDuration.Value)
                return $"{MinPlayDuration.Value} to {MaxPlayDuration.Value} {UnitOfMeasuresTime.GetSymbol(PlayDurationUnitCode ?? "")}";
            else if (MinPlayDuration.HasValue)
                return $"{MinPlayDuration}+ {UnitOfMeasuresTime.GetSymbol(PlayDurationUnitCode ?? "")}";
            else if (MaxPlayDuration.HasValue)
                return $"<={MaxPlayDuration.Value} {UnitOfMeasuresTime.GetSymbol(PlayDurationUnitCode ?? "")}";
            else return "-";

        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string PurchasedPriceText => PurchasedPrice.HasValue? PurchasedPrice.Value.ToString("$ #,##0.00") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string PurchasedDateText => PurchasedDate.HasValue? PurchasedDate.Value.ToString("dd-MMM-yyyy") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string SizeText 
    {
        get {
            if (SizeUnit != null && SizeLength.HasValue && SizeHeight.HasValue && SizeWidth.HasValue)
                return $"{SizeLength.Value:#,##0.#} {SizeUnit.UnitSymbol} x {SizeHeight.Value:#,##0.#} {SizeUnit.UnitSymbol} x {SizeWidth.Value:#,##0.#} {SizeUnit.UnitSymbol}";
            else
                return " - ";
        }
    }
    #endregion

    public Boardgame()
    {
        ContentItems = [];
        ObjectState = ObjectStates.NEW;
    }
}
