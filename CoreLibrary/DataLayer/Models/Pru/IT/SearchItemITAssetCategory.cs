namespace DataLayer.Models.Pru.IT;

using Dapper.Contrib.Extensions;

[DisplayName("IT Asset Category")]
public class SearchItemITAssetCategory
{
	#region *** DATABASE FIELDS ***
	public int Id { get; set; }
	public string? ObjectCode { get; set; }
	public string? ObjectName { get; set; }
	public int? ParentId { get; set; }
	public string? ParentCode { get; set; }
	public string? ParentName { get; set; }
	public string? HierarchyPath { get; set; }
	public string? AssetType { get; set; }

	public int? TotalCount { get; set; }
	public int? InUseCount { get; set; }
	public int? InStockCount { get; set; }
	public int? WrittenOffCount { get; set; }
	public int? DisposedTerminatedCount { get; set; }
	public int? BrokenCount { get; set; }
	public int? EOSCount { get; set; }
	public int? EOLCount { get; set; }
	#endregion

	public SearchItemITAssetCategory() : base()
    {
        
    }
}