using DataLayer.AuxComponents.DataAnnotations;

namespace DataLayer.Models.SysCore;

[Table("KhVillage"), DisplayName("Cambodia - Village")]
public class CambodiaVillage : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"KhVillage";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"kh_village";

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

    [MaxLength(255), StringUnicode(true)]
    public string? ReferenceText { get; set; }

    public string? PostalCode { get; set; }

    [MaxLength(255), StringUnicode(true)]
    public string? Note { get; set; }

    [MaxLength(255), StringUnicode(true)]
    public string? Remark { get; set; }

    public int? CambodiaCommuneId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false), ReadOnly(true)]
    public CambodiaCommune? Commune { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string CommuneNameEnText => Commune != null ? Commune.NameEn.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string CommuneNameKhText => Commune != null ? Commune.NameKh.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string DistrictNameEnText => Commune != null && Commune.District != null ? Commune.District.NameEn.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string DistrictNameKhText => Commune != null && Commune.District != null ? Commune.District.NameKh.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string ProvinceNameEnText => Commune != null && Commune.District != null && Commune.District.Province != null ? Commune.District.Province.NameEn.NonNullValue("-") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string ProvinceNameKhText => Commune != null && Commune.District != null && Commune.District.Province != null ? Commune.District.Province.NameKh.NonNullValue("-") : "-";
	#endregion
}