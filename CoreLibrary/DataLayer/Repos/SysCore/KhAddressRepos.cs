namespace DataLayer.Repos.SysCore;

public interface IKhAddressRepos : IBaseRepos<KhAddress>
{
	Task<List<KhAddress>> GetByLinkedObjectAsync(int linkedObjectId, string linkedObejctType);
}

public class KhAddressRepos(IDbContext dbContext) : BaseRepos<KhAddress>(dbContext, KhAddress.DatabaseObject), IKhAddressRepos
{
	public async Task<List<KhAddress>> GetByLinkedObjectAsync(int linkedObjectId, string linkedObejctType)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql
            .Where("t.IsDeleted=0")
            .Where("t.LinkedObjectId=@LinkedObjectId")
            .Where("t.LinkedObjectType=@LinkedObjectType");

        var sbSqlTempl = sbSql.AddTemplate($"SELECT * FROM {KhAddress.MsSqlTable} t /**where**/");

        param.Add("@LinkedObjectId", linkedObjectId);
        param.Add("@LinkedObjectType", linkedObejctType, DbType.AnsiString);

        using var cn = DbContext.DbCxn;

        List<KhAddress> dataList = (await cn.QueryAsync<KhAddress>(sbSqlTempl.RawSql, param)).ToList();

        return dataList;
    }
}