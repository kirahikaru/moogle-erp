namespace DataLayer.Repos.SystemCore;

public interface IDocumentTypeRepos : IBaseRepos<DocumentType>
{
	Task<List<DocumentType>> GetValidByObjectTypeNameAsync(string objectTypeName);
}

public class DocumentTypeRepos(IConnectionFactory connectionFactory) : BaseRepos<DocumentType>(connectionFactory, DocumentType.DatabaseObject), IDocumentTypeRepos
{
	public async Task<List<DocumentType>> GetValidByObjectTypeNameAsync(string objectTypeName)
    {
        var sql = $"SELECT dt.* FROM {DocumentType.MsSqlTable} dt " +
                  $"LEFT JOIN {SysObjDocType.MsSqlTable} sodt ON sodt.IsDeleted=0 AND sodt.DocumentTypeCode=dt.ObjectCode " +
                  $"WHERE dt.IsDeleted=0 AND sodt.ObjectName IS NOT NULL AND sodt.ObjectName=@ObjectName";

        var param = new { ObjectName = objectTypeName };

        using var cn = ConnectionFactory.GetDbConnection()!;

        var result = await cn.QueryAsync<DocumentType>(sql, param);
        return result.ToList();
    }
}