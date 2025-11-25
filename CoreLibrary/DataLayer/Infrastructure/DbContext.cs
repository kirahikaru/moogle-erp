using Npgsql;
using System.Data.Common;

namespace DataLayer.Infrastructure;

public interface IDbContext
{
	/// <summary>
	/// Database Connection
	/// </summary>
	IDbConnection DbCxn { get; }

	/// <summary>
	/// Database Type
	/// </summary>
	string DbType { get; }
}

public class DbContext(DatabaseConfig dbConfig) : IDbContext
{
	private readonly DatabaseConfig _dbConfig = dbConfig;
	private readonly string _dbType = dbConfig.DatabaseType.NonNullValue();

	/// <summary>
	/// Database Type
	/// </summary>
	public string DbType => _dbType;
	public IDbConnection DbCxn
	{
		get
		{
			if (_dbConfig.DatabaseType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
			{
				return new SqlConnection(_dbConfig.ConnectionString.NonNullValue());
			}
			else if (_dbConfig.DatabaseType.Is(DatabaseTypes.POSTGRESQL))
			{
				return new NpgsqlConnection(_dbConfig.ConnectionString.NonNullValue());
			}
			else
			{
				throw new NotSupportedException($"Database type '{_dbConfig.DatabaseType}' is not supported.");
			}
		}
	}
}
