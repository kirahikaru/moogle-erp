namespace DataLayer.Repos.SysCore;

public interface IMsngrConvoHistoryRepos : IBaseRepos<MessengerConvoHistory>
{

	/// <summary>
	/// Get by message id 
	/// </summary>
	/// <param name="messageId">id of message created from sending</param>
	/// <returns></returns>
	public Task<MessengerConvoHistory?> GetByMessageIdAsync(string messageId);

}

public class MsngrConvoHistoryRepos(IDbContext dbContext) : BaseRepos<MessengerConvoHistory>(dbContext, MessengerConvoHistory.DatabaseObject), IMsngrConvoHistoryRepos
{
	public async Task<MessengerConvoHistory?> GetByMessageIdAsync(string messageId)
    {
		ArgumentNullException.ThrowIfNull(messageId);

		SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("IsDeleted=0");
        sbSql.Where("ConversationId=@messageId");

        param.Add("@messageId", messageId, DbType.AnsiString);

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        return await cn.QueryFirstOrDefaultAsync<MessengerConvoHistory?>(sql, param);
    }
}