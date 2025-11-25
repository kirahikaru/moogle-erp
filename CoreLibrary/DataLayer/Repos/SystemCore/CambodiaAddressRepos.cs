namespace DataLayer.Repos.SystemCore;

public interface ICambodiaAddressRepos : IBaseRepos<CambodiaAddress>
{
	Task<List<CambodiaAddress>> GetByLinkedObjectAsync(int linkedObjectId, string linkedObejctType);
}

public class CambodiaAddressRepos(IConnectionFactory connectionFactory) : BaseRepos<CambodiaAddress>(connectionFactory, CambodiaAddress.DatabaseObject), ICambodiaAddressRepos
{
	public async Task<List<CambodiaAddress>> GetByLinkedObjectAsync(int linkedObjectId, string linkedObejctType)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql
            .Where("t.IsDeleted=0")
            .Where("t.LinkedObjectId=@LinkedObjectId")
            .Where("t.LinkedObjectType=@LinkedObjectType");

        var sbSqlTempl = sbSql.AddTemplate($"SELECT * FROM {CambodiaAddress.MsSqlTable} t /**where**/");

        param.Add("@LinkedObjectId", linkedObjectId);
        param.Add("@LinkedObjectType", linkedObejctType, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<CambodiaAddress> dataList = (await cn.QueryAsync<CambodiaAddress>(sbSqlTempl.RawSql, param)).ToList();

        return dataList;
    }
}