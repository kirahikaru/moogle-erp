namespace DataLayer.Models.SystemCore.NonPersistent;

public class DropDownListItem
{
    public int Id { get; set; }
    public int ObjectId { get; set; }
    public string? ObjectType { get; set; }
    public string? ObjectCode { get; set; }
    public string? ObjectName { get; set; }
    public string? ObjectNameEn { get; set; }
    public string? ObjectNameKh { get; set; }
    public string? HierarchyPath { get; set; }
}