namespace DataLayer.Models.SysCore.NonPersistent;

public class SqlSortCond
{
	public string? FieldName { get; set; }
	public string? SortingCommand { get; set; }
	public bool IsDecending { get; set; }

	public string GetSortCommand(string? tblVar)
	{
		if (string.IsNullOrEmpty(tblVar))
			return $"{FieldName} {(IsDecending ? "DESC" : "ASC")}";
		else
			return $"{tblVar}.{FieldName} {(IsDecending ? "DESC" : "ASC")}";
	}

	public SqlSortCond()
	{
	}

	public SqlSortCond(string fieldName, bool isDecending)
	{
		FieldName = fieldName;
		IsDecending = isDecending;
	}
}