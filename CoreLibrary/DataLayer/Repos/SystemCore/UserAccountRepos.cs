using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface IUserAccountRepos : IBaseRepos<UserAccount>
{
	Task<UserAccount?> GetByUserId(int userId);

	Task<List<UserAccount>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		int? organizationId = null,
		string? userId = null,
		string? name = null,
		string? employeeId = null,
		string? primaryPhoneNo = null,
		string? primaryEmail = null,
		string? username = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		int? organizationId = null,
		string? userId = null,
		string? name = null,
		string? employeeId = null,
		string? primaryPhoneNo = null,
		string? primaryEmail = null,
		string? username = null);
}

public class AccountRepos(IConnectionFactory connectionFactory) : BaseRepos<UserAccount>(connectionFactory, UserAccount.DatabaseObject), IUserAccountRepos
{
	public async Task<UserAccount?> GetByUserId(int id)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "ObjectId cannot be negative.");

        var sql = $"SELECT * FROM {UserAccount.MsSqlTable} WHERE IsDeleted = 0 AND UserId=@UserId";

        DynamicParameters param = new();
        param.Add("@UserId", id);

        using var cn = ConnectionFactory.GetDbConnection()!;

		return await cn.QuerySingleOrDefaultAsync<UserAccount>(sql, param);
    }

    public async Task<List<UserAccount>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        int? organizationId = null,
        string? userId = null,
        string? name = null,
        string? employeeId = null,
        string? primaryPhoneNo = null,
        string? primaryEmail = null,
        string? username = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (organizationId.HasValue && organizationId > 0)
        {
            sbSql.Where("t.OrganizationId=@OrganizationId");
            param.Add("@OrganizationId", organizationId);
        }

        if (!string.IsNullOrEmpty(userId))
        {
            sbSql.Where("LOWER(t.UserId) LIKE '%'+LOWER(@UserId)+'%'");
            param.Add("@UserId", userId, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(name))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@Name)+'%'");
            param.Add("@Name", name, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(primaryPhoneNo))
        {
            sbSql.Where("LOWER(t.PrimaryPhoneNo) LIKE '%'+@PrimaryPhoneNo+'%'");
            param.Add("@PrimaryPhoneNo", primaryPhoneNo, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(primaryEmail))
        {
            sbSql.Where("LOWER(t.primaryEmail) LIKE '%'+@PrimaryEmail+'%'");
            param.Add("@PrimaryEmail", primaryEmail, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(employeeId))
        {
            sbSql.Where("t.EmployeeId=@EmployeeId");
            param.Add("@EmployeeId", employeeId, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(username))
        {
            sbSql.Where("LOWER(t.UserName) LIKE '%'+LOWER(@UserName)+'%'");
            param.Add("@UserName", username, DbType.AnsiString);
        }
        #endregion
        sbSql.LeftJoin($"{User.MsSqlTable} u ON u.Id=t.UserId");
        sbSql.OrderBy("t.UserName ASC");

        string sql;
        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {UserAccount.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        }
        else
        {
            //throw new NotImplementedException();
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.*, u.* FROM {DbObject.MsSqlTable} t " +
                  $"INNER JOIN pg p ON p.Id=a.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<UserAccount> result = (await cn.QueryAsync<UserAccount, User, UserAccount>(
                sql, (account, user) =>
                {
                    account.User = user;
                    return account;

                }, param, splitOn: "Id"
            ).ConfigureAwait(false)).ToList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        int? organizationId = null,
        string? userId = null,
        string? name = null,
        string? employeeId = null,
        string? primaryPhoneNo = null,
        string? primaryEmail = null,
        string? username = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));
        
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (organizationId.HasValue && organizationId > 0)
        {
            sbSql.Where("t.OrganizationId=@OrganizationId");
            param.Add("@OrganizationId", organizationId);
        }

        if (!string.IsNullOrEmpty(userId))
        {
            sbSql.Where("LOWER(t.UserId) LIKE '%'+LOWER(@UserId)+'%'");
            param.Add("@UserId", userId, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(name))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@Name)+'%'");
            param.Add("@Name", name, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(primaryPhoneNo))
        {
            sbSql.Where("LOWER(t.PrimaryPhoneNo) LIKE '%'+@PrimaryPhoneNo+'%'");
            param.Add("@PrimaryPhoneNo", primaryPhoneNo, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(primaryEmail))
        {
            sbSql.Where("LOWER(t.primaryEmail) LIKE '%'+@PrimaryEmail+'%'");
            param.Add("@PrimaryEmail", primaryEmail, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(employeeId))
        {
            sbSql.Where("t.EmployeeId=@EmployeeId");
            param.Add("@EmployeeId", employeeId, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(username))
        {
            sbSql.Where("LOWER(t.UserName) LIKE '%'+LOWER(@UserName)+'%'");
            param.Add("@UserName", username, DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{User.MsSqlTable} u ON u.Id=t.UserId");

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)(Math.Ceiling(recordCount / pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(UserAccount).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}