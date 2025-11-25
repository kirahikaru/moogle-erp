namespace DataLayer.Repos.SystemCore;

public interface IMsngrConvoHistoryRepos : IBaseRepos<MessengerConvoHistory>
{

	/// <summary>
	/// Get by message id 
	/// </summary>
	/// <param name="messageId">id of message created from sending</param>
	/// <returns></returns>
	public Task<MessengerConvoHistory?> GetByMessageIdAsync(string messageId);

}

public class MsngrConvoHistoryRepos(IConnectionFactory connectionFactory) : BaseRepos<MessengerConvoHistory>(connectionFactory, MessengerConvoHistory.DatabaseObject), IMsngrConvoHistoryRepos
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

        using var cn = ConnectionFactory.GetDbConnection()!;

        return await cn.QueryFirstOrDefaultAsync<MessengerConvoHistory?>(sql, param);
    }
}