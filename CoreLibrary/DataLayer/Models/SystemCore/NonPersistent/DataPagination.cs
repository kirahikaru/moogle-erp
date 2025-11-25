namespace DataLayer.Models.SystemCore.NonPersistent;

public class DataPagination
{
    public string? ObjectType { get; set; }
    public int PageNo { get; set; }
	public int PageSize { get; set; }
    public int PageCount { get; set; }
    public int RecordCount { get; set; }
}