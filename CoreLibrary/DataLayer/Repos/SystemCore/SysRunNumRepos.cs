namespace DataLayer.Repos.SystemCore;

public interface ISysRunNumRepos : IBaseRepos<SysRunNum>
{
	/// <summary>
	/// Get next available ObjectCode (Running Number)
	/// </summary>
	/// <param name="objectType"></param>
	/// <param name="user"></param>
	/// <returns></returns>
	SysRunNum? GetAndLock(string objectType, User user);
	Task<SysRunNum?> GetAndLockAsync(string objectType, User user);
	Task<bool> ReleaseLockByTypeAsync(string objectType, User user);
	Task<bool> ReleaseLockAsync(int id, User user);
	Task<bool> ReleaseLockAsync(string objectCode, User user);
	Task<string> ClaimRunningNumberAsync(string objectCode, int linkedObjectId, User user);
}

public class SysRunNumRepos(IConnectionFactory connectionFactory) : BaseRepos<SysRunNum>(connectionFactory, SysRunNum.DatabaseObject), ISysRunNumRepos
{
	public SysRunNum? GetAndLock(string objectType, User user)
    {
        if (user == null)
            throw new Exception();

        var sql = $"SELECT TOP 1 * FROM {SysRunNum.MsSqlTable} WHERE IsDeleted=0 AND ObjectType=@ObjectType AND IsLocked<>1 AND LinkedObjectId IS NULL AND LinkedObjectType IS NULL ORDER BY Number ASC";
        var updSql = $"UPDATE {SysRunNum.MsSqlTable} " +
                     $"SET IsLocked=1, LockedByUserId=@UserId, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime WHERE Id=@Id";

        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

        var sqlParam = new { ObjectType = objectType };

        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();
        try
        {
            SysRunNum? result = cn.Query<SysRunNum>(sql, sqlParam, transaction: tran).SingleOrDefault();

            if (result != null && result.Id > 0)
            {
                var param = new { user.UserId, ModifiedUser = user.UserName, ModifiedDateTime = khTimestamp, result.Id };

                int updCount = cn.Execute(updSql, param, transaction: tran);

                if (updCount > 0)
                {
                    result.IsLocked = true;
                    result.LockedByUserId = user.UserId;
                }
            }

            tran.Commit();
            return result;
        }
        catch
        {
            tran.Rollback();
            return null;
        }
    }

    public async Task<SysRunNum?> GetAndLockAsync(string objectType, User user)
    {
        if (user == null)
            throw new Exception();

        var sql = $"SELECT TOP 1 * FROM {SysRunNum.MsSqlTable} WHERE IsDeleted=0 AND ObjectType=@ObjectType AND IsLocked<>1 AND LinkedObjectId IS NULL AND LinkedObjectType IS NULL ORDER BY Number ASC";
        var updSql = $"UPDATE {SysRunNum.MsSqlTable} " +
                     $"SET IsLocked=1, LockedByUserId=@UserId, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime WHERE Id=@Id";

        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

        var sqlParam = new { ObjectType = objectType };

        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();
        try
        {
            SysRunNum? result = cn.Query<SysRunNum>(sql, sqlParam, transaction: tran).SingleOrDefault();

            if (result != null && result.Id > 0)
            {
                var param = new { user.UserId, ModifiedUser = user.UserName, ModifiedDateTime = khTimestamp, result.Id };

                int updCount = await cn.ExecuteAsync(updSql, param, transaction: tran);

                if (updCount > 0)
                {
                    result.IsLocked = true;
                    result.LockedByUserId = user.UserId;
                }
            }

            tran.Commit();
            return result;
        }
        catch
        {
            tran.Rollback();
            return null;
        }
    }

    public async Task<bool> ReleaseLockByTypeAsync(string objectType, User user)
    {
        if (user == null || user.Id == 0)
            throw new ArgumentNullException(nameof(user), _errMsgResxMngr.GetString("Null", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        param.Add("@ModifiedUser", user.UserName, DbType.AnsiString);
        param.Add("@ModifiedDateTime", DateTime.UtcNow.AddHours(7));
        param.Add("@UserId", user.UserId, DbType.AnsiString);
        param.Add("@ObjectType", objectType, DbType.AnsiString);

        sbSql.Where("IsDeleted=0");
        sbSql.Where("ObjectType=@ObjectType");
        sbSql.Where("IsLocked=1");
        sbSql.Where("LockedByUserId=@UserId");
        sbSql.Where("LinkedObjectId IS NULL");
        sbSql.Where("LinkedObjectType IS NULL");

        var sql = sbSql.AddTemplate($"UPDATE {DbObject.MsSqlTable} SET IsLocked=0, LockedByUserId=NULL, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;
        int updCount = await cn.ExecuteAsync(sql, param).ConfigureAwait(false);

        return updCount > 0;
    }

    public async Task<bool> ReleaseLockAsync(int id, User user)
    {
        if (user == null || user.Id == 0)
            throw new ArgumentNullException(nameof(user), _errMsgResxMngr.GetString("Null", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        param.Add("@ModifiedUser", user.UserName, DbType.AnsiString);
        param.Add("@ModifiedDateTime", DateTime.UtcNow.AddHours(7));
        param.Add("@UserId", user.UserId, DbType.AnsiString);
        param.Add("@Id", id);

        sbSql.Where("IsDeleted=0");
        sbSql.Where("Id=@Id");
        sbSql.Where("IsLocked=1");
        sbSql.Where("LockedByUserId=@UserId");
        sbSql.Where("LinkedObjectId IS NULL");
        sbSql.Where("LinkedObjectType IS NULL");

        var sql = sbSql.AddTemplate($"UPDATE {DbObject.MsSqlTable} SET IsLocked=0, LockedByUserId=NULL, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;
        int updCount = await cn.ExecuteAsync(sql, param);

        return updCount > 0;
    }

    public async Task<bool> ReleaseLockAsync(string objectCode, User user)
    {
        if (user == null || user.Id == 0)
            throw new ArgumentNullException(nameof(user), _errMsgResxMngr.GetString("Null", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        param.Add("@ModifiedUser", user.UserName, DbType.AnsiString);
        param.Add("@ModifiedDateTime", DateTime.UtcNow.AddHours(7));
        param.Add("@UserId", user.UserId, DbType.AnsiString);
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.ObjectCode=@ObjectCode");
        sbSql.Where("t.IsLocked=1");
        sbSql.Where("t.LockedByUserId=@UserId");
        sbSql.Where("t.LinkedObjectId IS NULL");
        sbSql.Where("t.LinkedObjectType IS NULL");


        var sql = sbSql.AddTemplate($"UPDATE {DbObject.MsSqlTable} SET IsLocked=0, LockedByUserId=NULL, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        int updCount = await cn.ExecuteAsync(sql, param);

        return updCount > 0;
    }

    public async Task<string> ClaimRunningNumberAsync(string objectCode, int linkedObjectId, User user)
    {
        if (user == null || user.Id == 0)
            throw new ArgumentNullException(nameof(user), _errMsgResxMngr.GetString("Null", CultureInfo.CurrentUICulture));

        var sql = $"UDPATE {SysRunNum.MsSqlTable} " +
                  @"SET LinkedObjectId=@LinkedObjectId, LinkedObjectType=ObjectType, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime
                        WHERE IsDeleted=0 AND ObjectCode=@ObjectCode AND IsLocked=1 AND LockedByUserId=@UserId AND LinkedObjectId IS NULL AND LinkedObjectType IS NULL";

        var param = new { ObjectCode = objectCode, UserId = user.Id, ModifiedUser = user.UserName, ModifiedDateTime = DateTime.UtcNow.AddHours(7) };

        using var cn = ConnectionFactory.GetDbConnection()!;
        try
        {
            int updCount = await cn.ExecuteAsync(sql, param).ConfigureAwait(false);

            return updCount > 0 ? "" : $"Failed to update record in database. Cannot claim running number (Id={objectCode}) for LinkedObjectId={linkedObjectId} for user '{user.UserName}'.";
        }
        catch (Exception? ex)
        {
            string errMsg = "";

            while (ex != null)
            {
                errMsg += ex.Message + Environment.NewLine;
                ex = ex.InnerException;
            }

            return errMsg;
        }
    }
}