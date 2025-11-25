using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("KhProvince"), DisplayName("Cambodia - Province")]
public class CambodiaProvince : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"KhProvince";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"kh_province";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[MaxLength(150), StringUnicode(true)]
    public string? NameKh { get; set; }

    [MaxLength(150)]
    [Required(ErrorMessage = "Name (En) is required.")]
    public string? NameEn { get; set; }

    [MaxLength(2)]
    [Required(ErrorMessage = "'Code 2' is required.")]
    [RegularExpression(@"^[a-zA-Z]{2}$", ErrorMessage = "'Code 2' invalid format. Valid format input: Capital letter")]
    public string? Code2 { get; set; }

    [MaxLength(3)]
    [RegularExpression(@"^[a-zA-Z]{3}$", ErrorMessage = "'Code 3' invalid format. Valid format input: Capital letter")]
    public string? Code3 { get; set; }

    [RegularExpression(@"^[\d]{0,}$", ErrorMessage = "'Code 3' invalid format. Valid format input: Capital letter")]
    public string? PostalCode { get; set; }
    public bool IsCity { get; set; }
    public int KrongCount { get; set; }
    public int SrokCount { get; set; }
    public int KhanCount { get; set; }
    public int CommuneCount { get; set; }
    public int SangkatCount { get; set; }
    public int VillageCount { get; set; }
    public string? ReferenceText { get; set; }
    public string? Note { get; set; }
    public string? Remark { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public List<CambodiaDistrict> Districts { get; set; }

    [Computed, Write(false)]
    public List<CambodiaCommune> Communes { get; set; }

    [Computed, Write(false)]
    public List<CambodiaVillage> Villages { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion

    public CambodiaProvince() : base()
    {
        Districts = [];
        Communes = [];
        Villages = [];
    }
}