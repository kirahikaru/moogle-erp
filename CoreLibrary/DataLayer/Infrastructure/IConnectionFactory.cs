using MongoDB.Driver;
using Npgsql;
using System.Data.Common;

namespace DataLayer.Infrastructure; 
/// <summary>
/// 
/// </summary>
/// /// <remarks>
/// KB Source: https://www.c-sharpcorner.com/article/dapper-and-repository-pattern-in-web-api/
/// </remarks>
public interface IConnectionFactory : IDisposable 
{
	DbConnection? GetDbConnection();
	DbConnection? DbConnection { get; }
    NpgsqlConnection PosgreSqlDbConnection { get; }

	/// <summary>
	/// MS SQL Database Connection
	/// </summary>
	SqlConnection MsSqlDbConnection { get; }
	IMongoClient MongDbConnection { get; }
    string DatabaseType { get; }

    /// <summary>
    /// 
    /// </summary>
	string SqlDbEnvironment { get; }
    string MongoDbEnvironment { get; }
    string Db2Environment { get; }
    string PosgreSqlEnvironment { get; }
}
