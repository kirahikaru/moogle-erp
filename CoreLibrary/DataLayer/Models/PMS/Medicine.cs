using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.RMS;
using DataLayer.Models.SysCore;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.PMS;

[Table("[med].[Medicine]")]
public class Medicine : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.PHARMACY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Medicine).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "medicine";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'CODE' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Medicine Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

	/// <summary>
	/// Setup Values is in JSON file: dropdown-data-list>medicine-type-setup.json
	/// </summary>
	[Description("DropdownDataDriven")]
	public int? MedicineTypeId { get; set; }
    public string? MedicineTypeCode { get; set; }

    public string? PackageImagePath { get; set; }
    public string? ContentImagePath { get; set; }
    public int? ItemId { get; set; }

    [MaxLength(255), StringUnicode(true)]
    public string? LocalCallName { get; set; }
    
    public string? Barcode { get; set; }

    [MaxLength(1000)]
    public string? SubText { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? PackageUnitCode { get; set; }

    [MaxLength(50)]
    public string? ConsumableUnitCode { get; set; }

    [Range(0.00, 99999999999.99, ErrorMessage = "'Quantity' invalid format. Only positive number year allowed.")]
    [Precision(10, 2)]
    public double? ConsumableQuantity { get; set; }

    [MaxLength(3)]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Invalid format. Valid format: capital letter and only 3 characters long.")]
    public string? MfgCountryCode { get; set; }

    [MaxLength(1000)]
    public string? IngredientSummary { get; set; }

    [MaxLength(1000)]
    public string? UsageDirectionDetail { get; set; }

    [MaxLength(1000)]
    public string? DosageDetail { get; set; }

    [MaxLength(1000)]
    public string? CompositionSummary { get; set; }

    [MaxLength(500), StringUnicode(true)]
    public string? Remark { get; set; }

    [MaxLength(150)]
    public string? MfgCompanyName { get; set; }

    [MaxLength(150)]
    public string? LaboratoryName { get; set; }
    public string? LocalDistributor { get; set; }
    public string? ImportCompany { get; set; }

    [MaxLength(255)]
    public string? ReferenceLink { get; set; }

    public string? PriceCurrencyCode { get; set; }
    public decimal? RetailMarketPrice { get; set; }
    public decimal? RetailMarketPriceKhr { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? WholesalePriceKhr { get; set; }

	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Item? Item { get; set; }

	[Computed, Write(false)]
	public List<MedicineComposition> Compositions { get; set; }

	[Computed, Write(false)]
	public Country? ManufacturingCountry { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? PackageUnit { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? ConsumableUnit { get; set; }

	[Computed, Write(false)]
	public DropdownDataList? MedicineType { get; set; }

	[Computed, Write(false)]
	public List<AttachedImage> Images { get; set; }
    #endregion

    #region *** DYNAMIC FIELDS ***
    [Computed, Write(false), ReadOnly(true)]
    public string PackageContent
    {
        get
        {
            StringBuilder sb = new();

            if (PackageUnit != null)
                sb.Append(PackageUnit.ObjectName);

            if (ConsumableQuantity.HasValue)
            {
                if (sb.Length > 0)
                {
                    sb.Append(": ");
                }

                if (ConsumableQuantity.Value % 1 > 0)
                    sb.Append(ConsumableQuantity.Value.ToString("#,##0.00"));
                else
                    sb.Append(ConsumableQuantity.Value.ToString("#,##0"));

                if (ConsumableUnit != null)
                    sb.Append(" " + ConsumableUnit.ObjectName);
            }

            return sb.ToString();
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string RetailMarketPriceKhrText => RetailMarketPriceKhr != null ? $"{RetailMarketPriceKhr:#,##0}៛" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string WholeSalePriceKhrText => WholesalePriceKhr != null ? $"{WholesalePriceKhr:#,##0}៛" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string RetailMarketPriceText => WholesalePrice != null ? $"{WholesalePrice:#,##0}{Currencies.GetSymbol(PriceCurrencyCode)}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string WholeSalePriceText => WholesalePrice != null ? $"{WholesalePrice:#,##0}{Currencies.GetSymbol(PriceCurrencyCode)}" : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string TypeText => MedicineType != null ? MedicineType.ObjectName.NonNullValue() : "";
	#endregion

	public Medicine() : base()
    {
        Compositions = [];
        Images = [];
    }
}