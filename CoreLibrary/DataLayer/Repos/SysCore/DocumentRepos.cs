namespace DataLayer.Repos.SysCore;

public interface IDocumentRepos : IBaseRepos<Document>
{
	Task<List<Document>> GetByTypeAsync(string documentTypeCode);
	Task<List<Document>> GetByLinkedObjectAsync(int linkedObjId, string linkedObjType);
}

public class DocumentRepos(IDbContext dbContext) : BaseRepos<Document>(dbContext, Document.DatabaseObject), IDocumentRepos
{
	public async Task<List<Document>> GetByTypeAsync(string documentTypeCode)
    {
        var sql = $"SELECT * FROM {Document.MsSqlTable} WHERE IsDeleted=0 AND DocumentTypeCode=@DocumentTypeCode";

        using var cn = DbContext.DbCxn;
        var result = await cn.QueryAsync<Document>(sql, new { DocumentTypeCode = documentTypeCode });
        return result.AsList();
    }

    public async Task<List<Document>> GetByLinkedObjectAsync(int linkedObjId, string linkedObjType)
    {
        var sql = $"SELECT * FROM {Document.MsSqlTable} WHERE IsDeleted=0 AND LinkedObjectId=@LinkedObjectId AND LinkedObjectType=@LinkedObjectType";
        var param = new { LinkedObjectId = linkedObjId, LinkedObjectType = linkedObjType };

        using var cn = DbContext.DbCxn;
        var result = await cn.QueryAsync<Document>(sql, param);
        return result.AsList();
    }
}