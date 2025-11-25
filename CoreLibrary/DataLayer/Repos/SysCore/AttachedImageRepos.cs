namespace DataLayer.Repos.SysCore;

public interface IAttachedImageRepos : IBaseRepos<AttachedImage>
{
	Task<List<AttachedImage>> GetByLinkedObjectAsync(int linkedObjectId, string linkedObjectType);
}

public class AttachedImageRepos(IDbContext dbContext) : BaseRepos<AttachedImage>(dbContext, AttachedImage.DatabaseObject), IAttachedImageRepos
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

        using var cn = DbContext.DbCxn;

        var data = (await cn.QueryAsync<AttachedImage>(sql, param)).AsList();
        return data;
    }
}