namespace DataLayer.Repos.SystemCore;

public interface IAttachedImageRepos : IBaseRepos<AttachedImage>
{
	Task<List<AttachedImage>> GetByLinkedObjectAsync(int linkedObjectId, string linkedObjectType);
}

public class AttachedImageRepos(IConnectionFactory connectionFactory) : BaseRepos<AttachedImage>(connectionFactory, AttachedImage.DatabaseObject), IAttachedImageRepos
{
	public async Task<List<AttachedImage>> GetByLinkedObjectAsync(int linkedObjectId, string linkedObjectType)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.LinkedObjectType=@LinkedObjectType");

        DynamicParameters param = new();

        param.Add("@LinkedObjectType", linkedObjectType, DbType.AnsiString);

        sbSql.Where("t.LinkedObjectId=@LinkedObjectId");
        param.Add("@LinkedObjectId", linkedObjectId);

        string sql = sbSql.AddTemplate($"SELECT * FROM {AttachedImage.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<AttachedImage>(sql, param)).AsList();
        return data;
    }
}