using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;

namespace DataLayer.Models.SysCore;

//[Table("[dbo].[CambodiaDistrict]")]
[Table("KhDistrict"), DisplayName("Cambodia District")]
public class CambodiaDistrict : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"KhDistrict";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"kh_district";

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
    public string? NameEn { get; set; }
    public string? PostalCode { get; set; }
    /// <summary>
    /// Valid values K/D
    /// </summary>
    public string? Type { get; set; }

    public int CommuneCount { get; set; }
    public int SangkatCount { get; set; }
    public int VillageCount { get; set; }

    [MaxLength(255), StringUnicode(true)]
    public string? ReferenceText { get; set; }

    [MaxLength(255), StringUnicode(true)]
    public string? Note { get; set; }

    [MaxLength(255), StringUnicode(true)]
    public string? Remark { get; set; }

    public int? CambodiaProvinceId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public CambodiaProvince? Province { get; set; }

	[Computed, Write(false)]
	public List<CambodiaCommune> Communes { get; }

	[Computed, Write(false)]
	public List<CambodiaVillage> Villages { get; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string ProvinceNameEn => Province != null ? Province.NameEn.NonNullValue("-") : "-";

    [Computed, Write(false), ReadOnly(true)]
    public string ProvinceNameKh => Province != null ? Province.NameKh.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string TypeText => KhDistrictTypes.GetDisplayText(Type);


	#endregion

	public CambodiaDistrict() : base()
    {
        Communes = [];
        Villages = [];
	}
}