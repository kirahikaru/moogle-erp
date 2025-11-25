using DataLayer.AuxComponents.Extensions;
using MongoDB.Driver;
using Npgsql;
using System.Data.Common;

namespace DataLayer.Infrastructure; 
/// <summary>
/// 
/// </summary>
/// <remarks>
/// KB Source: https://www.c-sharpcorner.com/article/dapper-and-repository-pattern-in-web-api/
/// </remarks>
public class ConnectionFactory : IConnectionFactory 
{
    //private readonly string connectionString = ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString;
    private readonly string? _databaseType;
	private readonly string? _msSqlCxnStr;
    private readonly string? _mongoDbCxnStr;
    private readonly string? _db2CxnStr;
    private readonly string? _posgreSqlCxnStr;
    private DatabaseConfig? _sqlDbConfig;
    private DatabaseConfig? _mongoDbConfig;
    private DatabaseConfig? _db2DbConfig;
    private DatabaseConfig? _posgreSqlDbConfig;

    public ConnectionFactory(List<DatabaseConfig> dbConfigs, string databaseType)
    {
        _databaseType = databaseType;

		foreach (DatabaseConfig dbConfig in dbConfigs)
        {
            switch (dbConfig.DatabaseType)
            {
                case DatabaseTypes.AZURE_SQL:
                case DatabaseTypes.MSSQL:
                    _msSqlCxnStr = dbConfig.ConnectionString;
                    _sqlDbConfig = dbConfig;
                    break;
                case DatabaseTypes.MONGODB:
                    _mongoDbCxnStr = dbConfig.ConnectionString;
                    _mongoDbConfig = dbConfig;
                    break;
                case DatabaseTypes.IBM_DB2:
                    _db2CxnStr = dbConfig.ConnectionString;
                    _db2DbConfig = dbConfig;
                    break;
                case DatabaseTypes.POSTGRESQL:
                    _posgreSqlCxnStr = dbConfig.ConnectionString;
                    _posgreSqlDbConfig = dbConfig;
                    break;
            }
        }
    }

    public string DatabaseType => _databaseType.NonNullValue();

    public DbConnection? DbConnection
    {
        get
        {
            if (_databaseType == DatabaseTypes.AZURE_SQL || _databaseType == DatabaseTypes.MSSQL)
            {
                SqlConnection cn = new(_msSqlCxnStr);
                return cn;
            }
            else if (_databaseType == DatabaseTypes.POSTGRESQL)
            {
                NpgsqlConnection cn = new(_posgreSqlCxnStr);
                return cn;
			}

            return null;
        }
	}

	public SqlConnection MsSqlDbConnection
    {
        get
        {
            SqlConnection cn = new(_msSqlCxnStr);
            return cn;
        }
    }

    public NpgsqlConnection PosgreSqlDbConnection
	{
        get
        {
            NpgsqlConnection cn = new(_posgreSqlCxnStr);
            return cn;
		}
	}

	public string SqlDbEnvironment => _sqlDbConfig != null ? _sqlDbConfig.Environment.NonNullValue() : string.Empty;

    public string MongoDbEnvironment => _mongoDbConfig != null ? _mongoDbConfig.Environment.NonNullValue() : string.Empty;

    public string Db2Environment => _db2DbConfig != null ? _db2DbConfig.Environment.NonNullValue() : string.Empty;

    public string PosgreSqlEnvironment => _posgreSqlDbConfig != null ? _posgreSqlDbConfig.Environment.NonNullValue() : string.Empty;

    public IMongoClient MongDbConnection
    {
        get
        {
            IMongoClient cn = new MongoClient(_mongoDbCxnStr);
            return cn;
        }
    }

    //public SQLiteConnection DatabaseConnection {
    //    get {
    //        string dbPath = ConfigurationManager.AppSettings["SQLLiteDbPath"];

    //        if (!File.Exists(dbPath))
    //            throw new Exception("SQLite DB Not Found");

    //        SQLiteConnection cn = new SQLiteConnection("Data Source=" + dbPath);

    //        return cn;
    //    }
    //}

    public DbConnection? GetDbConnection()
	{
		if (_databaseType == DatabaseTypes.AZURE_SQL || _databaseType == DatabaseTypes.MSSQL)
		{
			SqlConnection cn = new(_msSqlCxnStr);
			return cn;
		}
		else if (_databaseType == DatabaseTypes.POSTGRESQL)
		{
			NpgsqlConnection cn = new(_posgreSqlCxnStr);
			return cn;
		}

		return null;
	}

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~ConnectionFactory() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        GC.SuppressFinalize(this);
    }
    #endregion
}


/// <summary>
/// Database Types Constants
/// </summary>
public class DatabaseTypes
{
	public const string AZURE_SQL = "AZURE-SQL";
	public const string MSSQL = "MSSQL";
	public const string MONGODB = "MONGODB";
	public const string IBM_DB2 = "IBM-DB2";
	public const string POSTGRESQL = "POSTGRESQL";
}