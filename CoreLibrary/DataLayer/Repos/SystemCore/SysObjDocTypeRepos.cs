namespace DataLayer.Repos.SystemCore;

public interface ISysObjDocTypeRepos : IBaseRepos<SysObjDocType>
{

	/// <summary>
	/// Get valid list of Document Type for an object
	/// </summary>
	/// <param name="objectTypeName"></param>
	/// <returns></returns>
	Task<List<SysObjDocType>> GetValidDocumentTypes(string objectTypeName);
}

public class SysObjDocTypeRepos(IConnectionFactory connectionFactory) : BaseRepos<SysObjDocType>(connectionFactory, SysObjDocType.DatabaseObject), ISysObjDocTypeRepos
{
	public async Task<List<SysObjDocType>> GetValidDocumentTypes(string objectTypeName)
    {
        var sql = $"SELECT * FROM {SysObjDocType.MsSqlTable} WHERE IsDeleted=0 AND ObjectName=@ObjectName";

        var param = new { ObjectName = objectTypeName };

        using var cn = ConnectionFactory.GetDbConnection()!;

        return (await cn.QueryAsync<SysObjDocType>(sql, param)).ToList();
    }
}