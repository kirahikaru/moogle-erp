using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.ManyToManyLink;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface IUserRepos : IBaseRepos<User>
{
	/// <summary>
	/// Get User with all object link by UserID
	/// </summary>
	Task<User?> GetFullByUserIdAsync(string userId);
	Task<User?> GetFullAsync(int id);

	Task<int> InsertFullAsync(User obj);

	Task<bool> UpdateFullAsync(User obj);

	Task<bool> HasRoleAsync(string userId, string roleCode);
	Task<bool> HasRolesAsync(string userId, List<string> roleList);

	Task<UserSessionInfo?> GetForSessionAsync(string userUserId);

    /// <summary>
    /// Get By Organization Structure
    /// </summary>
    /// <param name="orgStructId"></param>
    /// <returns></returns>
	Task<List<User>> GetByOrgStructAsync(int orgStructId);

	/// <summary>
	/// Get all users under all organization structure below given Organization Structure
	/// </summary>
	Task<List<User>> GetUnderOrgStructAsync(OrgStruct orgStruct);

	/// <summary>
	/// Get list of users under Current User's organization structure
	/// </summary>
	/// <param name="currentUserId">UserID of current user</param>
	/// <param name="includeUserLevel">include members in the same organization structure as {Current User}</param>
	/// <param name="includeAllLevelBelow">include all members in organization structure below Current User's organization structure</param>
	/// <param name="includeCurrentUserId"></param>
	/// <returns></returns>
	Task<List<User>> GetTeamMembersAsync(string currentUserId, bool includeUserLevel = true, bool includeAllLevelBelow = true, bool includeCurrentUserId = false);

	/// <summary>
	/// Get number of user under Current User's organization structure
	/// </summary>
	/// <param name="currentUserId">UserID of current user</param>
	/// <param name="includeUserLevel">include members in the same organization structure as {Current User}</param>
	/// <param name="includeAllLevelBelow">include all members in organization structure below Current User's organization structure</param>
	/// <param name="includeCurrentUserId"></param>
	/// <returns></returns>
	Task<int> GetTeamSizeAsync(string currentUserId, bool includeUserLevel = true, bool includeAllLevelBelow = true, bool includeCurrentUserId = false);

	Task<User?> GetUserByADAsync(string adAccount);

	User? GetByUserId(string userId);

	Task<User?> GetByUserIdAsync(string adAccount);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="organizationId"></param>
	/// <returns></returns>
	Task<List<User>> GetByOrganizationAsync(int organizationId);

	Task<List<Permission>> GetUserPermissionAsync(string userId);

	Task<List<User>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		int? organizationId = null,
		string? name = null,
		string? employeeId = null,
		string? username = null,
		string? phoneNumber = null,
		string? email = null,
		string? position = null,
		int? orgStructId = null,
		string? orgStructHierarchyPath = null,
		int? privacyAccessLevel = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		int? organizationId = null,
		string? name = null,
		string? employeeId = null,
		string? username = null,
		string? phoneNumber = null,
		string? email = null,
		string? position = null,
		int? orgStructId = null,
		string? orgStructHierarchyPath = null,
		int? privacyAccessLevel = null);

	Task<List<DropdownSelectItem>> GetEmployeeForDropdownAsync(int pgSize = 0, int pgNo = 0, string? searchText = null);

	Task<List<DropdownSelectItem>> GetForDropdownByTypeAsync(int pgSize = 0, int pgNo = 0, string userType = UserTypes.STAFF, string? searchText = null);

	#region FOR UI Control Population
	Task<List<DropDownListItem>> GetEmployeeUsersForDropdownAsync(string? searchText = null);
	#endregion
}

public class UserRepos(IConnectionFactory connectionFactory) : BaseRepos<User>(connectionFactory, User.DatabaseObject), IUserRepos
{
	public override List<User> GetAll()
    {
        string sql = $"SELECT * FROM {User.MsSqlTable} WHERE IsDeleted=0 ORDER BY UserName";

        using var cn = ConnectionFactory.GetDbConnection()!;

		return cn.Query<User>(sql).AsList();
    }

    public async Task<List<User>> GetByOrganizationAsync(int organizationId)
    {
        DynamicParameters param = new();

        var sql = $"SELECT * FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND OrganizationId=@OrganizationId";

        param.Add("@OrganizationId", organizationId);

        using var cn = ConnectionFactory.GetDbConnection()!;

        return (await cn.QueryAsync<User>(sql, param).ConfigureAwait(false)).AsList();
    }

    public async Task<UserSessionInfo?> GetForSessionAsync(string userUserId)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Select("'UserId'=t.Id")
            .Select("'UserUserID'=t.UserId")
            .Select("t.UserType")
            .Select("t.UserName")
            .Select("t.EmployeeId")
            .Select("t.TerminatedDateTime")
            .Select("t.IsEnabled")
            .Select("t.ReportToUserId")
            .Select("t.ConfidentialityLevel");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UserId=@UserUserId");
        param.Add("@UserUserId", userUserId, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;
		if (cn.State != ConnectionState.Open) cn.Open();
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        var obj = await cn.QueryFirstOrDefaultAsync<UserSessionInfo>(sql, param);
        return obj;
    }

    public async Task<User?> GetFullByUserIdAsync(string userId)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UserId=@UserId");

        sbSql.LeftJoin($"{UserAccount.MsSqlTable} ua ON ua.IsDeleted=0 AND ua.UserId=t.Id");
        sbSql.LeftJoin($"{OrgStruct.MsSqlTable} os ON os.Id=t.OrgStructId");
        sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=os.OrgStructTypeId");

        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        param.Add("@UserId", userId);

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<User, UserAccount, OrgStruct, OrgStructType, User>(
                    sql, (user, userAcc, orgStruct, orgStructType) =>
                    {
                        user.Account = userAcc;

                        if (orgStruct != null)
                        {
                            if (orgStructType != null)
                                orgStruct.Type = orgStructType;

                            user.OrgStruct = orgStruct;
                        }

                        return user;
                    }, param, splitOn: "Id")).FirstOrDefault();

        return data;
    }

    public async Task<User?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.LeftJoin($"{UserAccount.MsSqlTable} ua ON ua.IsDeleted=0 AND ua.UserId=t.Id");
        sbSql.LeftJoin($"{OrgStruct.MsSqlTable} os ON os.Id=t.OrgStructId");
        sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=os.OrgStructTypeId");

        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        param.Add("@Id", id);

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<User, UserAccount, OrgStruct, OrgStructType, User>(
                    sql, (user, userAcc, orgStruct, orgStructType) =>
                    {
                        user.Account = userAcc;

                        if (orgStruct != null)
                        {
                            if (orgStructType != null)
                                orgStruct.Type = orgStructType;

                            user.OrgStruct = orgStruct;
                        }

                        return user;
                    }, param, splitOn: "Id")).FirstOrDefault();

        return data;
    }

    public async Task<List<User>> GetByOrgStructAsync(int orgStructId)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.LeftJoin($"{OrgStruct.MsSqlTable} os ON os.Id=t.OrgStructId");
        sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=os.OrgStructTypeId");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.OrgStructId=@OrgStructId");

        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        param.Add("@OrgStructId", orgStructId);

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<User> users = (await cn.QueryAsync<User, OrgStruct, OrgStructType, User>(
                    sql, (user, orgStruct, orgStructType) =>
                    {
                        if (orgStruct != null)
                        {
                            if (orgStructType != null)
                                orgStruct.Type = orgStructType;

                            user.OrgStruct = orgStruct;
                        }

                        return user;
                    }, param, splitOn: "Id")).ToList();

        return users;
    }

    public async Task<List<User>> GetUnderOrgStructAsync(OrgStruct orgStruct)
    {
        if (orgStruct == null)
            return [];

        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("os.HierarchyPath LIKE @HierarchyPath+'%'");
        sbSql.LeftJoin($"{OrgStruct.MsSqlTable} os ON os.Id=u.OrgStructId");
        sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=os.OrgStructTypeId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        param.Add("@HierarchyPath", orgStruct.HierarchyPath);

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<User> users = (await cn.QueryAsync<User, OrgStruct, OrgStructType, User>(
                    sql, (user, orgStruct, orgStructType) =>
                    {
                        if (orgStruct != null)
                        {
                            if (orgStructType != null)
                                orgStruct.Type = orgStructType;

                            user.OrgStruct = orgStruct;
                        }

                        return user;
                    }, param, splitOn: "Id")).ToList();

        return users;
    }

    public async Task<List<User>> GetTeamMembersAsync(string currentUserId, bool includeUserLevel = true, bool includeAllLevelBelow = true, bool includeCurrentUserId = false)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        sbSql.LeftJoin($"{OrgStruct.MsSqlTable} os ON os.Id=t.OrgStructId");
        sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=os.OrgStructTypeId");

        if (includeUserLevel)
        {
            if (includeAllLevelBelow)
                sbSql.Where("os.HierarchyPath LIKE @HierarchyPath+'%'");
            else
                sbSql.Where("os.HierarchyPath=@HierarchyPath");
        }
        else if (includeAllLevelBelow)
        {
            sbSql.Where("os.HierarchyPath<>@HierarchyPath AND os.HierarchyPath LIKE @HierarchyPath+'%'");
        }

        if (!includeCurrentUserId)
        {
            sbSql.Where("t.UserId<>@UserId");
        }

        var getHierarchyPathSql = $"SELECT os.HierarchyPath FROM {User.MsSqlTable} u " +
                                  $"LEFT JOIN {OrgStruct.MsSqlTable} os ON os.Id=u.OrgStructId " +
                                  $"LEFT JOIN {OrgStructType.MsSqlTable} ost ON ost.Id=os.OrgStructTypeId " +
                                  $"WHERE u.IsDeleted=0 AND u.UserId=@UserId";

        using var cn = ConnectionFactory.GetDbConnection()!;
        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        string? hierarchyPath = cn.QuerySingleOrDefault<string>(getHierarchyPathSql, new { UserId = currentUserId });

        if (string.IsNullOrEmpty(hierarchyPath))
            throw new Exception($"hierarchy path for user ({currentUserId}) not found.");

        param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);
        param.Add("@UserId", currentUserId, DbType.AnsiString);

        List<User> users = (await cn.QueryAsync<User, OrgStruct, OrgStructType, User>(
                    sql, (user, orgStruct, orgStructType) =>
                    {
                        if (orgStruct != null)
                        {
                            if (orgStructType != null)
                                orgStruct.Type = orgStructType;

                            user.OrgStruct = orgStruct;
                        }

                        return user;
                    }, param, splitOn: "Id")).ToList();

        return users;
    }

    public async Task<int> GetTeamSizeAsync(string currentUserId, bool includeUserLevel = true, bool includeAllLevelBelow = true, bool includeCurrentUserId = false)
    {
        if (!includeUserLevel && !includeAllLevelBelow)
            return includeCurrentUserId ? 1 : 0;

        SqlBuilder sbSql = new();
        sbSql.Where("u.IsDeleted = 0");

        sbSql.LeftJoin($"{OrgStruct.MsSqlTable} os ON os.Id=u.OrgStructId");
        sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=os.OrgStructTypeId");

        var sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {User.MsSqlTable} u /**leftjoin**/ /**where**/").RawSql;

        if (includeUserLevel)
        {
            if (includeAllLevelBelow)
                sbSql.Where("os.HierarchyPath LIKE @HierarchyPath+'%'");
            else
                sbSql.Where("os.HierarchyPath=@HierarchyPath");
        }
        else if (includeAllLevelBelow)
        {
            sbSql.Where("os.HierarchyPath<>@HierarchyPath AND os.HierarchyPath LIKE @HierarchyPath+'%'");
        }

        if (includeAllLevelBelow)
        {

        }

        if (!includeCurrentUserId)
        {
            sbSql.Where("u.UserId<>@UserId");
        }

        var getHierarchyPathSql = $"SELECT os.HierarchyPath FROM {User.MsSqlTable} u " +
                                  $"LEFT JOIN {OrgStruct.MsSqlTable} os ON os.Id=u.OrgStructId " +
                                  $"LEFT JOIN {OrgStructType.MsSqlTable} ost ON ost.Id=os.OrgStructTypeId " +
                                  $"WHERE u.IsDeleted=0 AND u.UserId=@UserId";

        using var cn = ConnectionFactory.GetDbConnection()!;

        string? hierarchyPath = cn.ExecuteScalar<string>(getHierarchyPathSql, new { UserId = currentUserId });

        if (string.IsNullOrEmpty(hierarchyPath))
            throw new Exception($"hierarchy path for user ({currentUserId}) not found.");

        DynamicParameters param = new();
        param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);
        param.Add("@UserId", currentUserId, DbType.AnsiString);

        int count = await cn.ExecuteScalarAsync<int>(sql, param);

        return count;
    }

    public User? GetByUserId(string userId)
    {
        var sql = $"SELECT * FROM {User.MsSqlTable} WHERE IsDeleted=0 AND UserId=@UserId";

        var parameters = new { UserId = userId };

        using var cn = ConnectionFactory.GetDbConnection()!;

        return cn.QuerySingleOrDefault<User>(sql, parameters);
    }

    public async Task<User?> GetByUserIdAsync(string adAccount)
    {
        var sql = $"SELECT * FROM {User.MsSqlTable} WHERE IsDeleted=0 AND UserId=@UserId";

        var parameters = new { UserId = adAccount };

        using var cn = ConnectionFactory.GetDbConnection()!;

        return await cn.QuerySingleOrDefaultAsync<User>(sql, parameters);
    }

    public async Task<User?> GetUserByADAsync(string adAccount)
    {
        var sql = $"SELECT * FROM {User.MsSqlTable} WHERE IsDeleted=0 AND UserId=@UserId";
        var parameters = new { @UserId = adAccount };

        using var cn = ConnectionFactory.GetDbConnection()!;

        return await cn.QuerySingleOrDefaultAsync<User>(sql, parameters).ConfigureAwait(false);
    }

    public async Task<List<Permission>> GetUserPermissionAsync(string userId)
    {
        var sql = $"SELECT DISTINCT p.Id, p.[Name] FROM {User.MsSqlTable} u " +
                  $"LEFT JOIN {UserRole.MsSqlTable} ur ON u.Id=ur.UserId " +
                  $"LEFT JOIN {Role.MsSqlTable} r ON r.Id = ur.RoleId " +
                  $"LEFT JOIN {RolePermission.MsSqlTable} rp ON rp.RoleId = r.Id " +
                  $"LEFT JOIN {Permission.MsSqlTable} p ON p.Id = rp.PermissionId " +
                  $"WHERE u.UserId=@UserId";

        var parameters = new { UserId = userId };

        using var cn = ConnectionFactory.GetDbConnection()!;

        return (await cn.QueryAsync<Permission>(sql, parameters).ConfigureAwait(false)).ToList();
    }

    public async Task<bool> HasRoleAsync(string userId, string roleCode)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleCode))
            return false;

        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UserUserID=@UserID");
        sbSql.Where("t.RoleCode=@RoleCode");
        sbSql.Where("u.Id IS NOT NULL");
        sbSql.Where("r.Id IS NOT NULL");

        sbSql.LeftJoin($"{User.MsSqlTable} u ON u.IsDeleted=0 AND u.Id=t.UserId");
        sbSql.LeftJoin($"{Role.MsSqlTable} r ON r.IsDeleted=0 AND r.Id=t.RoleId");

        DynamicParameters param = new();
        param.Add("@UserID", userId, DbType.AnsiString);
        param.Add("@RoleCode", roleCode, DbType.AnsiString);

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {UserRole.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        int count = await cn.ExecuteScalarAsync<int>(sql, param);
        return count > 0;
    }

    public async Task<bool> HasRolesAsync(string userId, List<string> roleList)
    {
        if (string.IsNullOrEmpty(userId) || !roleList.Any())
            return false;

        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UserUserID=@UserID");
        sbSql.Where("t.RoleCode IN @RoleList");
        sbSql.Where("u.Id IS NOT NULL");
        sbSql.Where("r.Id IS NOT NULL");

        sbSql.LeftJoin($"{User.MsSqlTable} u ON u.IsDeleted=0 AND u.Id=t.UserId");
        sbSql.LeftJoin($"{Role.MsSqlTable} r ON r.IsDeleted=0 AND r.Id=t.RoleId");

        DynamicParameters param = new();
        param.Add("@UserID", userId, DbType.AnsiString);
        param.Add("@RoleList", roleList);

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {UserRole.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        int count = await cn.ExecuteScalarAsync<int>(sql, param);
        return count > 0;
    }

	public override async Task<KeyValuePair<int, IEnumerable<User>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0, string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

        if (ConnectionFactory.DatabaseType.Is(DatabaseTypes.POSTGRESQL))
        {
            sbSql.Where("t.is_deleted=false");

			#region Form Search Conditions
			if (!string.IsNullOrEmpty(searchText))
			{
				if (searchText.StartsWith("id:"))
				{
					sbSql.Where("UPPER(t.object_code) LIKE '%'+@search_text+'%'");
					param.Add("@search_text", searchText.Replace("id:", "", StringComparison.CurrentCultureIgnoreCase), DbType.AnsiString);
				}
				else
				{
					sbSql.Where("(UPPER(t.object_name) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.object_code) LIKE '%'+UPPER(@search_text)+'%')");
					param.Add("@search_text", searchText, DbType.AnsiString);
				}
			}

			if (excludeIdList != null && excludeIdList.Count != 0)
			{
				sbSql.Where("t.id NOT IN @exclude_id_list");
				param.Add("@exclude_id_list", excludeIdList);
			}
			#endregion

			sbSql.LeftJoin($"{OrgStruct.MsSqlTable} orgStruct ON orgStruct.is_deleted=false AND orgStruct.id=t.org_struct_id");
		}
        else
        {
			sbSql.Where("t.IsDeleted=0");

			#region Form Search Conditions
			if (!string.IsNullOrEmpty(searchText))
			{
				if (searchText.StartsWith("id:"))
				{
					sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%'");
					param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.CurrentCultureIgnoreCase), DbType.AnsiString);
				}
				else
				{
					sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
					param.Add("@SearchText", searchText, DbType.AnsiString);
				}
			}

			if (excludeIdList != null && excludeIdList.Count != 0)
			{
				sbSql.Where("t.Id NOT IN @ExcludeIdList");
				param.Add("@ExcludeIdList", excludeIdList);
			}
			#endregion

			sbSql.LeftJoin($"{Country.MsSqlTable} natl ON natl.IsDeleted=0 AND natl.ObjectCode=t.NatlCtyCode");
		}

        if (filterConds is not null && filterConds.Any())
        {
            foreach (SqlFilterCond filter in filterConds)
            {
                sbSql.Where(filter.GetFilterSqlCommand("t"));
                if (filter.Parameters.ParameterNames.Any())
                    param.AddDynamicParams(filter.Parameters);
            }
        }

        if (sortConds is not null && sortConds.Any())
        {
			foreach (SqlSortCond orderBy in sortConds)
				sbSql.OrderBy(orderBy.GetSortCommand("t"));
		}
        else
        {
			foreach (string order in GetSearchOrderbBy())
				sbSql.OrderBy(order);
		}
            

		string sql = "";
        string sqlCount = "";

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
            if (ConnectionFactory.DatabaseType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
            {
				param.Add("@PageSize", pgSize);
				param.Add("@PageNo", pgNo);

				sql = sbSql.AddTemplate(
				    $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				    $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;


				sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
			}
            else if (ConnectionFactory.DatabaseType == DatabaseTypes.POSTGRESQL)
            {
				param.Add("@pg_size", pgSize);
				param.Add("@pg_no", pgNo);

				sql = sbSql.AddTemplate(
					$"SELECT * FROM {User.PgTable} t /**leftjoin**/ /**where**/ /**orderby**/ LIMIT @pg_size OFFSET @pg_size * (@pg_no - 1)").RawSql;

				sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {User.PgTable} t /**where**/").RawSql;
			}
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = await cn.QueryAsync<User, OrgStruct, User>(sql,
			(obj, orgStruct) => {
				obj.OrgStruct = orgStruct;

				return obj;
			}, param, splitOn: "Id");

		
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	/// <summary>
	/// Search users
	/// </summary>
	public async Task<List<User>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        int? organizationId = null,
        string? name = null,
        string? employeeId = null,
        string? username = null,
        string? phoneNumber = null,
        string? email = null,
        string? position = null,
        int? orgStructId = null,
        string? orgStructHierarchyPath = null,
        int? privacyAccessLevel = null)
    {

        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(phoneNumber))
        {
            sbSql.Where("t.PrimaryPhoneNo LIKE '%'+@PrimaryPhoneNo+'%' OR u.SecondaryPhoneNo LIKE '%'+@SecondaryPhoneNo+'%')");
            param.Add("@PrimaryPhoneNo", phoneNumber, DbType.AnsiString);
            param.Add("@SecondaryPhoneNo", phoneNumber, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(email))
        {
            sbSql.Where("(t.PrimaryEmail LIKE '%'+@PrimaryEmail+'%' OR t.SecondaryEmail LIKE '%'+@SecondaryEmail+'%')");
            param.Add("@PrimaryEmail", email, DbType.AnsiString);
            param.Add("@SecondaryEmail", email, DbType.AnsiString);
        }

        if (organizationId.HasValue && organizationId > 0)
        {
            sbSql.Where("t.OrganizationId=@OrganizationId");
            param.Add("@OrganizationId", organizationId);
        }

        if (!string.IsNullOrEmpty(name))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@Name)+'%'");
            param.Add("@Name", name, DbType.AnsiString);
        }

        if (privacyAccessLevel != null && privacyAccessLevel > 0)
        {
            sbSql.Where("t.PrivacyAccessLevel=@PrivacyAccessLevel");
            param.Add("@PrivacyAccessLevel", privacyAccessLevel);
        }

        if (!string.IsNullOrEmpty(employeeId))
        {
            sbSql.Where("LOWER(t.EmployeeId) LIKE '%'+LOWER(@EmployeeId)+'%'");
            param.Add("@EmployeeId", employeeId, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(username))
        {
            sbSql.Where("LOWER(t.UserName) LIKE '%'+LOWER(@UserName)+'%'");
            param.Add("@UserName", username, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(position))
        {
            sbSql.Where("LOWER(t.Position) LIKE '%'+@Position+'%'");
            param.Add("@Position", position, DbType.AnsiString);
        }

        if (orgStructId != null && orgStructId > 0)
        {
            sbSql.Where("t.OrgStructId=@OrgStructId");
            param.Add("@OrgStructId", orgStructId);
        }

        if (!string.IsNullOrEmpty(orgStructHierarchyPath))
        {
            sbSql.Where("os.HierarchyPath LIKE @HierarchyPath+'%'");
            param.Add("@HierarchyPath", orgStructHierarchyPath, DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{OrgStruct.MsSqlTable} os ON os.Id=u.OrgStructId");

        sbSql.OrderBy("t.Surname ASC");
        sbSql.OrderBy("t.GivenName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql =  sbSql.AddTemplate($"SELECT * FROM {User.MsSqlTable} t /**where**/").RawSql;
        }
        else
        {
            //throw new NotImplementedException();
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<User>(sql, param).ConfigureAwait(false)).ToList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        int? organizationId = null,
        string? name = null,
        string? employeeId = null,
        string? username = null,
        string? phoneNumber = null,
        string? email = null,
        string? position = null,
        int? orgStructId = null,
        string? orgStructHierarchyPath = null,
        int? privacyAccessLevel = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(phoneNumber))
        {
            sbSql.Where("(t.PrimaryPhoneNo LIKE '%'+@PrimaryPhoneNo+'%' OR u.SecondaryPhoneNo LIKE '%'+@SecondaryPhoneNo+'%')");
            param.Add("@PrimaryPhoneNo", phoneNumber, DbType.AnsiString);
            param.Add("@SecondaryPhoneNo", phoneNumber, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(email))
        {
            sbSql.Where("(t.PrimaryEmail LIKE '%'+@PrimaryEmail+'%' OR t.SecondaryEmail LIKE '%'+@SecondaryEmail+'%')");
            param.Add("@PrimaryEmail", email, DbType.AnsiString);
            param.Add("@SecondaryEmail", email, DbType.AnsiString);
        }

        if (organizationId.HasValue && organizationId > 0)
        {
            sbSql.Where("t.OrganizationId=@OrganizationId");
            param.Add("@OrganizationId", organizationId);
        }

        if (!string.IsNullOrEmpty(name))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@Name)+'%'");
            param.Add("@Name", name, DbType.AnsiString);
        }

        if (privacyAccessLevel != null && privacyAccessLevel > 0)
        {
            sbSql.Where("t.PrivacyAccessLevel=@PrivacyAccessLevel");
            param.Add("@PrivacyAccessLevel", privacyAccessLevel);
        }

        if (!string.IsNullOrEmpty(employeeId))
        {
            sbSql.Where("LOWER(r.EmployeeId) LIKE '%'+LOWER(@EmployeeId)+'%'");
            param.Add("@EmployeeId", employeeId, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(username))
        {
            sbSql.Where("LOWER(t.UserName) LIKE '%'+LOWER(@UserName)+'%'");
            param.Add("@UserName", username, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(position))
        {
            sbSql.Where("LOWER(t.Position) LIKE '%'+LOWER(@Position)+'%'");
            param.Add("@Position", position, DbType.AnsiString);
        }

        if (orgStructId != null && orgStructId > 0)
        {
            sbSql.Where("t.OrgStructId=@OrgStructId");
            param.Add("@OrgStructId", orgStructId);
        }

        if (!string.IsNullOrEmpty(orgStructHierarchyPath))
        {
            sbSql.Where("os.HierarchyPath LIKE @HierarchyPath+'%'");
            param.Add("@HierarchyPath", orgStructHierarchyPath, DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{OrgStruct.MsSqlTable} os ON os.Id=t.OrgStructId");
        sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=os.OrgStructTypeId");

        var sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param).ConfigureAwait(false);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(User).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<DropdownSelectItem>> GetEmployeeForDropdownAsync(int pgSize = 0, int pgNo = 0, string? searchText = null)
    {
        if (pgNo < 0 || pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("t.Id")
            .Select("'Key'=t.UserId")
            .Select("'Value'=t.UserName");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UserType=@UserType");

        param.Add("@UserType", UserTypes.STAFF, DbType.AnsiString);
        
        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.UserName) LIKE '%'+UPPER(@SearchText)+'%'");
            param.Add("@SearchText", searchText);
        }

        sbSql.OrderBy("t.UserName ASC");

        string sql;

        if (pgSize == 0 && pgNo == 0)
        {
            sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            sql = sbSql.AddTemplate($"; WITH pg AS(SELECT t.Id FROM {User.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                    $"SELECT /**select**/ FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**where**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return dataList;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdownByTypeAsync(int pgSize = 0, int pgNo = 0, string userType = UserTypes.STAFF, string? searchText = null)
    {
        if (pgNo < 0 || pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("t.Id")
            .Select("'Key'=t.UserId")
            .Select("'Value'=t.UserName");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UserType=@UserType");

        param.Add("@UserType", userType, DbType.AnsiString);

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.UserName) LIKE '%'+UPPER(@SearchText)+'%'");
            param.Add("@SearchText", searchText);
        }

        sbSql.OrderBy("t.UserName ASC");

        string sql;

        if (pgSize == 0 && pgNo == 0)
        {
            sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            sql = sbSql.AddTemplate($"; WITH pg AS(SELECT t.Id FROM {User.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                    $"SELECT /**select**/ FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**where**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return dataList;
    }

    public async Task<int> InsertFullAsync(User obj)
    {
        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            int objId = await cn.InsertAsync(obj, tran);

            if (objId > 0)
            {
                if (obj.Account != null)
                {
                    obj.Account.UserId = objId;
                    obj.Account.UserName = obj.UserName;
                    obj.Account.CreatedDateTime = obj.CreatedDateTime;
                    obj.Account.CreatedUser = obj.CreatedUser;
                    obj.Account.ModifiedDateTime = obj.ModifiedDateTime;
                    obj.Account.ModifiedUser = obj.ModifiedUser;

                    int accountId = await cn.InsertAsync(obj.Account!, tran);
                }

                foreach (UserRole ur in obj.Roles)
                {
                    if (ur.IsDeleted)
                        continue;

                    ur.UserId = obj.Id;
                    ur.UserUserID = obj.UserId;
                    ur.UserName = obj.UserName;
                    ur.CreatedDateTime = obj.CreatedDateTime;
                    ur.CreatedUser = obj.CreatedUser;
                    ur.ModifiedDateTime = obj.ModifiedDateTime;
                    ur.ModifiedUser = obj.ModifiedUser;

                    int userRoleId = await cn.InsertAsync(ur);
                }
            }
            else
                throw new Exception("Error inserting user full.");

            tran.Commit();
            return objId;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateFullAsync(User obj)
    {
        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            bool isUpd = await cn.UpdateAsync(obj, tran);

            if (isUpd)
            {
                if (obj.Account != null)
                {
                    if (obj.Account.Id > 0)
                    {
                        obj.Account.UserId = obj.Id;
                        obj.Account.UserName = obj.UserName;
                        obj.Account.ModifiedDateTime = obj.ModifiedDateTime;
                        obj.Account.ModifiedUser = obj.ModifiedUser;

                        bool isAccountUpd = await cn.UpdateAsync(obj.Account!, tran);
                    }
                    else
                    {
                        obj.Account.UserId = obj.Id;
                        obj.Account.UserName = obj.UserName;
                        obj.Account.CreatedDateTime = obj.CreatedDateTime;
                        obj.Account.CreatedUser = obj.CreatedUser;
                        obj.Account.ModifiedDateTime = obj.ModifiedDateTime;
                        obj.Account.ModifiedUser = obj.ModifiedUser;

                        int accountId = await cn.InsertAsync(obj.Account!, tran);
                    }
                    
                }

                foreach (UserRole ur in obj.Roles)
                {
                    if (ur.Id == 0 && ur.IsDeleted)
                        continue;

                    if (ur.Id == 0)
                    {
                        ur.UserId = obj.Id;
                        ur.UserUserID = obj.UserId;
                        ur.UserName = obj.UserName;
                        ur.CreatedDateTime = obj.CreatedDateTime;
                        ur.CreatedUser = obj.CreatedUser;
                        ur.ModifiedDateTime = obj.ModifiedDateTime;
                        ur.ModifiedUser = obj.ModifiedUser;

                        int userRoleId = await cn.InsertAsync(ur);
                    }
                    else
                    {
                        ur.UserId = obj.Id;
                        ur.UserUserID = obj.UserId;
                        ur.UserName = obj.UserName;
                        ur.ModifiedDateTime = obj.ModifiedDateTime;
                        ur.ModifiedUser = obj.ModifiedUser;

                        bool isUserRoleUpd = await cn.UpdateAsync(ur);
                    }
                }
            }
            else
                throw new Exception("Error inserting user full.");

            tran.Commit();
            return isUpd;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    #region FOR UI Control Population
    public async Task<List<DropDownListItem>> GetEmployeeUsersForDropdownAsync(string? searchText = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("'ObjectId'=t.Id");
        sbSql.Select("'ObjectCode'=t.UserId");
        sbSql.Select("'ObjectName'=t.UserName");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UserType=@UserType");

        param.Add("@UserType", "EMPLOYEE", DbType.AnsiString);

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where($"LOWER(t.UserName) LIKE '%'+LOWER(@UserName)+'%'");
            param.Add("@UserName", searchText, DbType.AnsiString);
        }

        using var cn = ConnectionFactory.GetDbConnection()!;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        List<DropDownListItem> result = (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();

        return result;
    }
    #endregion
}