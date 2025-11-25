namespace DataLayer.Models.SysCore.NonPersistent;

[DisplayName("Cambodia Village")]
public class SearchItemCambodiaVillage
{
    public int? Id { get; set; }
    public string? ObjectCode { get; set; }
    public string? NameEn { get; set; }
    public string? NameKh { get; set; }
    public string? Postalcode { get; set; }
    public int? CommuneId { get; set; }
    public string? CommuneCode { get; set; }
    public string? CommuneNameEn { get; set; }
    public string? CommuneNameKh { get; set; }
    public string? DistrictCode { get; set; }
    public int? DistrictId { get; set; }
    public string? DistrictNameEn { get; set; }
    public string? DistrictNameKh { get; set; }
    public string? ProvinceCode { get; set; }
    public int? ProvinceId { get; set; }
    public string? ProvinceNameEn { get; set; }
    public string? ProvinceNameKh { get; set; }
}