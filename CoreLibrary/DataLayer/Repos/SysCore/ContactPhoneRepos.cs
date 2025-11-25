namespace DataLayer.Repos.SysCore;

public interface IContactPhoneRepos : IBaseRepos<ContactPhone>
{
	Task<int> InsertMultipleAsync(List<ContactPhone> contacts);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="linkedObjectId"></param>
	/// <param name="linkedObjectType"></param>
	/// <param name="channel"></param>
	/// <returns></returns>
	Task<List<ContactPhone>> GetByLinkedObjectAsync(int? linkedObjectId, string linkedObjectType, string channel = "");
}

public class ContactPhoneRepos(IDbContext dbContext) : BaseRepos<ContactPhone>(dbContext, ContactPhone.DatabaseObject), IContactPhoneRepos
{
	public async Task<int> InsertMultipleAsync(List<ContactPhone> contacts)
    {
        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();
        try
        {
            int result = await cn.InsertAsync(contacts, tran).ConfigureAwait(false);
            tran.Commit();
            return result;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<List<ContactPhone>> GetByLinkedObjectAsync(
        int? linkedObjectId, 
        string linkedObjectType, 
        string channel = "")
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.LinkedObjectType=@LinkedObjectType");

        param.Add("@LinkedObjectType", linkedObjectType);

        var sql = sbSql.AddTemplate($"SELECT Id FROM {Contact.MsSqlTable} t /**where**/").RawSql;

        if (linkedObjectId.HasValue)
        {
            sbSql.Where("t.LinkedObjectId=@LinkedObjectId");
            param.Add("@LinkedObjectId", linkedObjectId.Value);
        }

        if (channel.IsAtLeast(1))
        {
            sbSql.Where("t.Channel=@Channel");
            param.Add("@Channel", channel);
        }
        else sql += " AND Channel IS NULL";

        using var cn = DbContext.DbCxn;

        List<ContactPhone> result = (await cn.QueryAsync<ContactPhone>(sql, param)).AsList();

        return result;
    }
}