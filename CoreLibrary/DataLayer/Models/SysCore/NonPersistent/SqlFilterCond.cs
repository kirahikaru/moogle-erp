namespace DataLayer.Models.SysCore.NonPersistent;

public class SqlFilterCond
{
	public string? CombineConditionOperator { get; set; }
	public string? FieldName { get; set; }
	public string? FilterOperator { get; set; }
	public string? FilterText { get; set; }
	public object? FilterValue { get; set; }
	public string? SqlCommand { get; set; }
	public DynamicParameters Parameters { get; set; }

	public SqlFilterCond()
	{
		Parameters = new DynamicParameters();
	}

	public string GetFilterSqlCommand(string? tblVar = null)
	{
		if (!string.IsNullOrEmpty(SqlCommand))
			return SqlCommand;

		if (!string.IsNullOrEmpty(tblVar))
		{
			return $"{tblVar}.{FieldName} {FilterOperator} @{FieldName}";
		}
		else
		{
			return $"{FieldName} {FilterOperator} @{FieldName}";
		}
	}
}
