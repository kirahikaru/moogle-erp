namespace DataLayer.Models;

public interface IParentChildHierarchyObject
{
    int? ParentId { get; set; }
    string? ParentCode { get; set; }
    string? HierarchyPath { get; set; }
}