using DataLayer.Models.SysCore.ManyToManyLink;
using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Repos.SysCore;

public interface IRoleRepos : IBaseRepos<Role>
{
	Task<Role?> GetFullAsync(int id);

	Task<int> InsertFullAsync(Role obj);
	Task<bool> UpdateFullAsync(Role obj);

	Task<List<Role>> GetByUserAsync(int userId);

	Task<List<Permission>> GetPermissions(int roleId);

	Task<List<RoleSysMod>> GetBySystemModuleAsync(int systemModuleId);

	Task<List<Role>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		int? organizationId = null,
		string? objectCode = null,
		string? objectName = null,
		bool? isEnabled = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		int? organizationId = null,
		string? objectCode = null,
		string? objectName = null,
		bool? isEnabled = null);

	Task<DropdownSelectDataResult> GetForSelectMenuAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null);
}

public class RoleRepos(IDbContext dbContext) : BaseRepos<Role>(dbContext, Role.DatabaseObject), IRoleRepos
{
	public async Task<Role?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");
		param.Add("@Id", id);

		using var cn = DbContext.DbCxn;

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		Role? obj = await cn.QueryFirstOrDefaultAsync<Role?>(sql, param);

		if (obj != null)
		{
			SqlBuilder sbSqlSystemModule = new();
			DynamicParameters paramSystemModule = new();

			sbSqlSystemModule.Where("t.IsDeleted=0");
			sbSqlSystemModule.LeftJoin($"{SystemModule.MsSqlTable} sm ON sm.Id=t.SystemModuleId");
			sbSqlSystemModule.Where("t.RoleId=@RoleId");

			paramSystemModule.Add("@RoleId", obj.Id);

			string sqlSystemModule = sbSqlSystemModule.AddTemplate($"SELECT * FROM {RoleSysMod.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
			List<RoleSysMod> systemModuleList = (await cn.QueryAsync<RoleSysMod, SystemModule, RoleSysMod>(sqlSystemModule, (obj, systemModule) =>
			{
				obj.SystemModule = systemModule;
				return obj;
			}, paramSystemModule, splitOn: "Id")).AsList();

			obj.AssignedSystemModules = systemModuleList;

			// User list from UserRole
			SqlBuilder sbSqlUser = new();
			DynamicParameters paramUser = new();

			sbSqlUser.Where("t.IsDeleted=0");
			sbSqlUser.LeftJoin($"{User.MsSqlTable} u ON u.Id=t.UserId");
			sbSqlUser.Where("t.RoleId=@RoleId");

			paramUser.Add("@RoleId", obj.Id);

			string sqlUser = sbSqlUser.AddTemplate($"SELECT * FROM {UserRole.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
			List<UserRole> userList = (await cn.QueryAsync<UserRole, User, UserRole>(sqlUser, (obj, user) =>
			{
				obj.User = user;
				return obj;
			}, paramUser, splitOn: "Id")).AsList();

			obj.AssignedUsers = userList;
		}

		return obj;
	}

    public async Task<List<Role>> GetByUserAsync(int userId)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where($"t.Id IN (SELECT Id FROM {UserRole.MsSqlTable} WHERE IsDeleted=0 AND UserId=@UserId)");
		sbSql.OrderBy("t.ObjectName ASC");

		param.Add("@UserId", userId);

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

        using var cn = DbContext.DbCxn;
		var dataList = (await cn.QueryAsync<Role>(sql, param)).AsList();
		return dataList;
    }

    public async Task<int> InsertFullAsync(Role obj)
	{
		DateTime timestamp = DateTime.UtcNow.AddHours(7);

		using var cn = DbContext.DbCxn;

		// <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
		if (cn.State != ConnectionState.Open) cn.Open();

		using var tran = cn.BeginTransaction();

		try
		{
			obj.CreatedDateTime = timestamp;
			obj.ModifiedDateTime = timestamp;

			int objId = await cn.InsertAsync(obj, tran);

			if (objId <= 0)
				throw new Exception("Failed to insert System Module object.");

			foreach (RoleSysMod systemModule in obj.AssignedSystemModules)
			{
				systemModule.RoleId = objId;

				if (systemModule.Id > 0)
				{
					systemModule.ModifiedUser = obj.ModifiedUser;
					systemModule.ModifiedDateTime = obj.ModifiedDateTime;
					bool isRoleSystemModuleUpdated = await cn.UpdateAsync(systemModule, tran);
				}
				else if (!systemModule.IsDeleted)
				{
					systemModule.CreatedUser = obj.CreatedUser;
					systemModule.CreatedDateTime = obj.CreatedDateTime;
					systemModule.ModifiedUser = obj.ModifiedUser;
					systemModule.ModifiedDateTime = obj.ModifiedDateTime;
					int roleSystemModuleId = await cn.InsertAsync(systemModule, tran);
					systemModule.Id = roleSystemModuleId;
				}
			}

			foreach (UserRole user in obj.AssignedUsers)
			{
				user.RoleId = objId;
				user.RoleCode = obj.ObjectCode;
				user.RoleName = obj.ObjectName;

				if (user.Id > 0)
				{
					user.ModifiedUser = obj.ModifiedUser;
					user.ModifiedDateTime = obj.ModifiedDateTime;
					bool isUserRoleUpdated = await cn.UpdateAsync(user, tran);
				}
				else if (!user.IsDeleted)
				{
					user.CreatedUser = obj.CreatedUser;
					user.CreatedDateTime = obj.CreatedDateTime;
					user.ModifiedUser = obj.ModifiedUser;
					user.ModifiedDateTime = obj.ModifiedDateTime;
					int userRoleId = await cn.InsertAsync(user, tran);
					user.Id = userRoleId;
				}
			}

			tran.Commit();
			return objId;
		}
		catch
		{
			tran.Rollback();
			throw;
		}
	}

	public async Task<bool> UpdateFullAsync(Role obj)
	{
		if (obj.Id <= 0)
			throw new Exception("Object is not existing.");

		DateTime timestamp = DateTime.UtcNow.AddHours(7);

		using var cn = DbContext.DbCxn;

		// <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
		if (cn.State != ConnectionState.Open) cn.Open();

		using var tran = cn.BeginTransaction();

		try
		{
			obj.ModifiedDateTime = timestamp;

			bool isUpdated = await cn.UpdateAsync(obj, tran);

			if (!isUpdated)
				throw new Exception("Failed to insert System Module object.");

			foreach (RoleSysMod systemModule in obj.AssignedSystemModules)
			{
				systemModule.RoleId = obj.Id;

				if (systemModule.Id > 0)
				{
					systemModule.ModifiedUser = obj.ModifiedUser;
					systemModule.ModifiedDateTime = obj.ModifiedDateTime;
					bool isRoleUpdated = await cn.UpdateAsync(systemModule, tran);
				}
				else if (!systemModule.IsDeleted)
				{
					systemModule.CreatedUser = obj.ModifiedUser;
					systemModule.CreatedDateTime = obj.ModifiedDateTime;
					systemModule.ModifiedUser = obj.ModifiedUser;
					systemModule.ModifiedDateTime = obj.ModifiedDateTime;
					int roleId = await cn.InsertAsync(systemModule, tran);
					systemModule.Id = roleId;
				}
			}

			foreach (UserRole user in obj.AssignedUsers)
			{
				user.RoleId = obj.Id;
				user.RoleCode = obj.ObjectCode;
				user.RoleName = obj.ObjectName;

				if (user.Id > 0)
				{
					user.ModifiedUser = obj.ModifiedUser;
					user.ModifiedDateTime = obj.ModifiedDateTime;
					bool isRoleUpdated = await cn.UpdateAsync(user, tran);
				}
				else if (!user.IsDeleted)
				{
					user.CreatedUser = obj.ModifiedUser;
					user.CreatedDateTime = obj.ModifiedDateTime;
					user.ModifiedUser = obj.ModifiedUser;
					user.ModifiedDateTime = obj.ModifiedDateTime;
					int roleId = await cn.InsertAsync(user, tran);
					user.Id = roleId;
				}
			}

			tran.Commit();
			return isUpdated;
		}
		catch
		{
			tran.Rollback();
			throw;
		}
	}

	public async Task<List<RoleSysMod>> GetBySystemModuleAsync(int systemModuleId)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.LeftJoin($"{DbObject.MsSqlTable} r ON r.Id=t.RoleId");
		sbSql.Where("t.SystemModuleId=@SystemModuleId");

		param.Add("@SystemModuleId", systemModuleId);
		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {RoleSysMod.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
		List<RoleSysMod> dataList = (await cn.QueryAsync<RoleSysMod, Role, RoleSysMod>(sql, (obj, role) =>
		{
			obj.Role = role;
			return obj;
		}, param, splitOn: "Id")).AsList();
		return dataList;
	}

	public async Task<List<Permission>> GetPermissions(int roleId)
    {
        if (roleId <= 0)
            throw new ArgumentOutOfRangeException(nameof(roleId), "ObjectId cannot be negative.");

        var sql = $"SELECT pm.* " +
                  $"FROM {RolePermission.MsSqlTable} rolPm " +
                  $"LEFT JOIN {Permission.MsSqlTable} pm ON pm.IsDeleted = 0 AND pm.Id = rolPm.PermissionId " +
                  $"WHERE rolPm.IsDeleted = 0 AND rolPm.RoleId = @RoleId ";

        var parameters = new { @RoleId = roleId };
        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<Permission>(sql, parameters).ConfigureAwait(false)).ToList();
    }

    public async Task<List<Role>> SearchAsync(
         int pgSize = 0,
         int pgNo = 0,
         int? organizationId = null,
         string? objectCode = null,
         string? objectName = null,
         bool? isEnabled = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        //Implement lower-case convert for text-type base search to lower is to ensure searching is case-insensitive
        //since now PCLAAPP DB is a unicode enabled database, i.e. all search are case sensitive

        if (organizationId.HasValue && organizationId > 0)
        {
            sbSql.Where("t.OrganizationId=@OrganizationId");
            param.Add("@OrganizationId", organizationId);
        }

        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+LOWER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName);
        }

        if (isEnabled.HasValue)
        {
            sbSql.Where("t.IsEnabled=@isEnabled");
            param.Add("@IsEnabled", isEnabled.Value);
        }

        sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            //throw new NotImplementedException();
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=u.Id /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<Role>(sql, param)).AsList();
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        int? organizationId = null,
        string? objectCode = null,
        string? objectName = null,
        bool? isEnabled = null)
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

        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+LOWER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (isEnabled.HasValue)
        {
            sbSql.Where("t.IsEnabled=@isEnabled");
            param.Add("@IsEnabled", isEnabled.HasValue);
        }

        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(Role).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

	public async Task<DropdownSelectDataResult> GetForSelectMenuAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Select("t.Id");
		sbSql.Select("'Key'=t.ObjectCode");
		sbSql.Select("'Value'=t.ObjectName");

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.IsEnabled=1");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else if (searchText.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("code:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Any())
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		string sql;
		sbSql.OrderBy("t.ObjectName ASC");

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				  $"SELECT /**select**/ FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		DropdownSelectDataResult dataResult = new();

		dataResult.Items = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

		dataResult.PagingInfo = new()
		{
			ObjectType = typeof(Role).Name,
			PageSize = pgSize,
			PageCount = pageCount,
			RecordCount = (int)recordCount
		};

		return dataResult;
	}
}