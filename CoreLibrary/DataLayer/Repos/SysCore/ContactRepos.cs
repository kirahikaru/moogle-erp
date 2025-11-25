namespace DataLayer.Repos.SysCore;

public interface IContactRepos : IBaseRepos<Contact>
{
	Task<int> InsertMultipleAsync(List<Contact> contacts);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="linkedObjectId"></param>
	/// <param name="linkedRecordID">Some time record has ID in string and object and not integer Id field. If Object is from this PCLA Solution Model this field would mostly be null.</param>
	/// <param name="linkedObjectType"></param>
	/// <param name="channel"></param>
	/// <returns></returns>
	Task<List<Contact>> GetByLinkedObjectAsync(int? linkedObjectId, string linkedObjectRecordID, string linkedObjectType, string channel = "");

	/// <summary>
	/// Get list of Id of contact that is existing
	/// </summary>
	/// <param name="linkedObjectId"></param>
	/// <param name="linkedRecordID"></param>
	/// <param name="linkedObjectType"></param>
	/// <param name="channel"></param>
	/// <param name="customChannel"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	Task<List<int>> GetExistingContactsAsync(int? linkedObjectId, string linkedObjectRecordID, string linkedObjectType, string channel, string customChannel, string value);
}

public class ContactRepos(IDbContext dbContext) : BaseRepos<Contact>(dbContext, Contact.DatabaseObject), IContactRepos
{
	public async Task<int> InsertMultipleAsync(List<Contact> contacts)
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

    public async Task<List<int>> GetExistingContactsAsync(
        int? linkedObjectId, 
        string linkedObjectRecordID, 
        string linkedObjectType, 
        string channel, 
        string customChannel, 
        string value)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.LinkedObjectType=@LinkedObjectType");
        param.Add("@LinkedObjectType", linkedObjectType);

        if (linkedObjectId.HasValue)
        {
            sbSql.Where("t.LinkedObjectId=@LinkedObjectId");
            param.Add("@LinkedObjectId", linkedObjectId.Value);
        }

        if (linkedObjectRecordID.IsAtLeast(1))
        {
            sbSql.Where("t.LinkedObjectRecordID=@LinkedObjectRecordID");
            param.Add("@LinkedObjectRecordID", linkedObjectRecordID);
        }

        if (channel.IsAtLeast(1))
        {
            sbSql.Where("t.Channel=@Channel");
            param.Add("@Channel", channel);
        }
        else sbSql.Where("t.Channel IS NULL");

        if (customChannel.IsAtLeast(1))
        {
            sbSql.Where("t.CustomChannel=@CustomChannel");
            param.Add("@CustomChannel", customChannel);
        }
        else sbSql.Where("t.CustomChannel IS NULL");

        using var cn = DbContext.DbCxn;
        var sql = sbSql.AddTemplate($"SELECT Id FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        List<int> ids = (await cn.QueryAsync<int>(sql, param)).AsList();

        return ids;
    }

    public async Task<List<Contact>> GetByLinkedObjectAsync(
        int? linkedObjectId, 
        string linkedObjectRecordID, 
        string linkedObjectType, 
        string channel = "")
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.LinkedObjectType=@LinkedObjectType");

        param.Add("@LinkedObjectType", linkedObjectType);

        if (linkedObjectId.HasValue)
        {
            sbSql.Where("t.LinkedObjectId=@LinkedObjectId");
            param.Add("@LinkedObjectId", linkedObjectId.Value);
        }

        if (linkedObjectRecordID.IsAtLeast(1))
        {
            sbSql.Where("t.LinkedObjectRecordID=@LinkedObjectRecordID");
            param.Add("@LinkedObjectRecordID", linkedObjectRecordID, DbType.AnsiString);
        }

        if (channel.IsAtLeast(1))
        {
            sbSql.Where("(t.Channel=@Channel OR t.CustomChannel=@Channel)");
            param.Add("@Channel", channel);
        }

        using var cn = DbContext.DbCxn;
        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        return (await cn.QueryAsync<Contact>(sql, param)).AsList();
    }
}