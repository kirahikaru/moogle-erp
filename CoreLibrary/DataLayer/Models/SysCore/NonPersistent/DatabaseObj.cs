namespace DataLayer.Models.SysCore.NonPersistent;

public class DatabaseObj
{
	public string Schema { get; set; }

	/// <summary>
	/// MS SQL Server Table Name
	/// </summary>
	public string MsSqlTableName { get; set; }

	/// <summary>
	/// PostgreSQL Table Name
	/// </summary>
	public string PgTableName { get; set; }

	public DatabaseObj(string schema, string msSqlTblName, string pgTblName)
	{
		Schema = schema;
		MsSqlTableName = msSqlTblName;
		PgTableName = pgTblName;
	}

	/// <summary>
	/// Full MS SQL Server Table Name
	/// </summary>
	public string MsSqlTable => string.IsNullOrEmpty(Schema) ? $"[{MsSqlTableName}]" : $"[{Schema}].[{MsSqlTableName}]";

	/// <summary>
	/// Full PostgreSQL Table Name
	/// </summary>
	public string PgTable => string.IsNullOrEmpty(Schema) ? $"\"{PgTableName.ToLower()}\"" : $"{Schema}.\"{PgTableName.ToLower()}\"";

	public static string GetTable(string schema, string tblName, string databaseType)
	{
		if (databaseType.Is(DatabaseTypes.POSTGRESQL))
			return string.IsNullOrEmpty(schema) ? $"\"{tblName}\"" : $"{schema}.\"{tblName}\"";
		else if (databaseType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
			return string.IsNullOrEmpty(schema) ? $"[{tblName}]" : $"[{schema}].[{tblName}]";
		else
			throw new NotImplementedException();
	}
}