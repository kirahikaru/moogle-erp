using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.AuxComponents.Extensions;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

//[Table("[dbo].[CambodiaCommune]")]
[Table("KhCommune"), DisplayName("Cambodia - Commune")]
public class CambodiaCommune : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"KhCommune";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"kh_commune";

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

    public int? VillageCount { get; set; }

    [MaxLength(255), StringUnicode(true)]
    public string? ReferenceText { get; set; }

    public string? PostalCode { get; set; }

    [MaxLength(255), StringUnicode(true)]
    public string? Note { get; set; }

    /// <summary>
    /// Valid values S/C
    /// </summary>
    public string? Type { get; set; }

    [MaxLength(255), StringUnicode(true)]
    public string? Remark { get; set; }

    public int? CambodiaDistrictId { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false), ReadOnly(true)]
	public CambodiaDistrict? District { get; set; }

	[Computed, Write(false), ReadOnly(true)]
	public List<CambodiaVillage> Villages { get; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string DistrictNameEnText => District != null ? District.NameEn.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string DistrictNameKhText => District != null ? District.NameKh.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string ProvinceNameEnText => District != null && District.Province != null ? District.Province.NameEn.NonNullValue("-") : "-";

    [Computed, Write(false), ReadOnly(true)]
    public string ProvinceNameKhText => District != null && District.Province != null ? District.Province.NameKh.NonNullValue("-") : "-";
    #endregion

    public CambodiaCommune() : base()
    {
        Villages = [];
    }
}