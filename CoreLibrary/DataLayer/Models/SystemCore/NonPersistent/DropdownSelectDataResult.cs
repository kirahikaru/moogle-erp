namespace DataLayer.Models.SystemCore.NonPersistent;

public class DropdownSelectDataResult
{
	public List<DropdownSelectItem> Items { get; set; }
	public DataPagination? PagingInfo { get; set; }

	public DropdownSelectDataResult()
	{
		Items = [];
	}
}
