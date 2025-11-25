namespace DataLayer.Models.SysCore;
/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[dbo].[CambodiaAddress]"), DisplayName("Cambodia Address")]
public class CambodiaAddress : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"KhAddress";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"kh_address";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? LinkedObjectId { get; set; }
    public string? LinkedObjectType { get; set; }
    public string? UnitFloor { get; set; }
    public string? StreetNo { get; set; }
    public string? Landmark { get; set; }
	public string? DistrictName { get; set; }
	public string? DistrictNameKh { get; set; }
	public string? ProvinceName { get; set; }
	public string? ProvinceNameKh { get; set; }
	public string? CommuneName { get; set; }
	public string? CommuneNameKh { get; set; }
	public string? VillageName { get; set; }
	public string? VillageNameKh { get; set; }
	public string? PostalCode { get; set; }

    public int? CambodiaProvinceId { get; set; }
    public int? CambodiaDistrictId { get; set; }
    public int? CambodiaCommuneId { get; set; }
    public int? CambodiaVillageId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed]
    public string DisplayText
    {
        get
        {
			StringBuilder sb = new();

			if (!string.IsNullOrEmpty(UnitFloor))
				sb.Append($"#{UnitFloor}, ");

			if (!string.IsNullOrEmpty(StreetNo))
				sb.Append($"{StreetNo}, ");

			if (!string.IsNullOrEmpty(Landmark))
				sb.Append($"{Landmark}, ");

			if (!string.IsNullOrEmpty(VillageName))
				sb.Append($"{VillageName}, ");

			if (!string.IsNullOrEmpty(CommuneName))
				sb.Append($"{CommuneName}, ");

			if (!string.IsNullOrEmpty(DistrictName))
				sb.Append($"{DistrictName}, ");

			if (!string.IsNullOrEmpty(ProvinceName))
				sb.Append($"{ProvinceName} ");

			if (!string.IsNullOrEmpty(PostalCode))
				sb.Append($"({PostalCode})");

			return sb.ToString();
		}
    }

	[Computed]
	public string DisplayTextKh
	{
		get
		{
			StringBuilder sb = new();

			if (!string.IsNullOrEmpty(UnitFloor))
				sb.Append($"#{UnitFloor}, ");

			if (!string.IsNullOrEmpty(StreetNo))
				sb.Append($"St. {StreetNo}, ");

			if (!string.IsNullOrEmpty(Landmark))
				sb.Append($"{Landmark}, ");

			if (!string.IsNullOrEmpty(VillageNameKh))
				sb.Append($"{VillageNameKh}, ");

			if (!string.IsNullOrEmpty(VillageNameKh))
				sb.Append($"{VillageNameKh}, ");

			if (!string.IsNullOrEmpty(DistrictNameKh))
				sb.Append($"{DistrictNameKh}, ");

			if (!string.IsNullOrEmpty(ProvinceNameKh))
				sb.Append($"{ProvinceNameKh} ");

			if (!string.IsNullOrEmpty(PostalCode))
				sb.Append($"({PostalCode})");

			return sb.ToString();
		}
	}
	#endregion
}