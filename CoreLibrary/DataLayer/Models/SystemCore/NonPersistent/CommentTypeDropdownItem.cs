namespace DataLayer.Models.SystemCore.NonPersistent;

public class CommentTypeDropdownItem
{
    public int Id { get; set; }
    public string? ObjectCode { get; set; }
    public string? ObjectName { get; set; }
    public string? ParentCode { get; set; }
    public string? ParentName { get; set; }
    public string? HierarchyPath { get; set; }
}