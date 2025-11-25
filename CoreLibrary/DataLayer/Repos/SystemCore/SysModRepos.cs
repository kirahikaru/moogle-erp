using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface ISysModRepos : IBaseRepos<SystemModule>
{
	Task<SystemModule?> GetFullAsync(int id);

	Task<int> InsertFullAsync(SystemModule obj);
	Task<bool> UpdateFullAsync(SystemModule obj);

	Task<bool> IsDuplicatedModulePathAsync(int objId, string modulePath);

	Task<List<SystemModule>> GetListAsync(List<int> systemModuleIdList);

	Task<List<SystemModule>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		bool? isEnabled = null,
		string? parentHierarchyPath = null,
		string? modulePath = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		bool? isEnabled = null,
		string? parentHierarchyPath = null,
		string? modulePath = null);

	Task<List<RoleSysMod>> GetByRoleAsync(int roleId);

	Task<List<DropDownListItem>> GetValidParentsAsync(int objectId, string objectCode, string hierarchyPath);

	Task<List<DropDownListItem>> GetForDropdownAsync(int pgSize = 0, int pgNo = 0, string? searchText = null);

	Task<List<DropdownSelectItem>> GetAssignedObjectClassesAsync(string? excludeClassName = null);

	Task<DataResult<SystemModule>> GetForSelectMenuAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null);
}

public class SysModRepos(IConnectionFactory connectionFactory) : BaseRepos<SystemModule>(connectionFactory, SystemModule.DatabaseObject), ISysModRepos
{
	public async Task<bool> IsDuplicatedModulePathAsync(int objId, string modulePath)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id<>@Id");
		sbSql.Where("t.ModulePath=@ModulePath");

		param.Add("@Id", objId);
		param.Add("@ModulePath", modulePath, DbType.AnsiString);

		using var cn = ConnectionFactory.GetDbConnection()!;
		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		return await cn.ExecuteScalarAsync<int>(sql, param) > 0;
	}

	public async Task<SystemModule?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0")
			.Where("t.Id=@Id");

		param.Add("@Id", id);

		sbSql.LeftJoin($"{SystemModule.MsSqlTable} pr ON pr.Id=t.ParentId");

		using var cn = ConnectionFactory.GetDbConnection()!;

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		List<SystemModule> dataList = (await cn.QueryAsync<SystemModule, SystemModule, SystemModule>(sql,
										(obj, parent) =>
										{
											obj.Parent = parent;
											return obj;
										}, param, splitOn: "Id")).AsList();

		if (dataList != null && dataList.Any())
		{
			SqlBuilder sbSqlRole = new();
			DynamicParameters paramRole = new();

			sbSqlRole.Where("t.IsDeleted=0");
			sbSqlRole.LeftJoin($"{Role.MsSqlTable} r ON r.Id=t.RoleId");
			sbSqlRole.Where("t.SystemModuleId=@SystemModuleId");

			paramRole.Add("@SystemModuleId", dataList[0].Id);

			string sqlRole = sbSqlRole.AddTemplate($"SELECT * FROM {RoleSysMod.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
			List<RoleSysMod> roleList = (await cn.QueryAsync<RoleSysMod, Role, RoleSysMod>(sqlRole, (obj, role) =>
			{
				obj.Role = role;
				return obj;
			}, paramRole, splitOn: "Id")).AsList();

			dataList[0].AssignedRoles = roleList;
			return dataList[0];
		}
		else return null;
	}

	public async Task<int> InsertFullAsync(SystemModule obj)
	{
		DateTime timestamp = DateTime.UtcNow.AddHours(7);

		using var cn = ConnectionFactory.GetDbConnection()!;

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

			foreach (RoleSysMod role in obj.AssignedRoles)
			{
				role.SystemModuleId = objId;

				if (role.Id > 0)
				{
					role.ModifiedUser = obj.ModifiedUser;
					role.ModifiedDateTime = obj.ModifiedDateTime;
					bool isRoleUpdated = await cn.UpdateAsync(role, tran);
				}
				else if (!role.IsDeleted)
				{
					role.CreatedUser = obj.CreatedUser;
					role.CreatedDateTime = obj.CreatedDateTime;
					role.ModifiedUser = obj.ModifiedUser;
					role.ModifiedDateTime = obj.ModifiedDateTime;
					int roleId = await cn.InsertAsync(role, tran);
					role.Id = roleId;
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

	public async Task<bool> UpdateFullAsync(SystemModule obj)
	{
		if (obj.Id <= 0)
			throw new Exception("Object is not existing.");

		DateTime timestamp = DateTime.UtcNow.AddHours(7);

		using var cn = ConnectionFactory.GetDbConnection()!;

		// <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
		if (cn.State != ConnectionState.Open) cn.Open();

		using var tran = cn.BeginTransaction();

		try
		{
			obj.ModifiedDateTime = timestamp;

			bool isUpdated = await cn.UpdateAsync(obj, tran);

			if (!isUpdated)
				throw new Exception("Failed to insert System Module object.");

			foreach (RoleSysMod role in obj.AssignedRoles)
			{
				role.SystemModuleId = obj.Id;

				if (role.Id > 0)
				{
					role.ModifiedUser = obj.ModifiedUser;
					role.ModifiedDateTime = obj.ModifiedDateTime;
					bool isRoleUpdated = await cn.UpdateAsync(role, tran);
				}
				else if (!role.IsDeleted)
				{
					role.CreatedUser = obj.ModifiedUser;
					role.CreatedDateTime = obj.ModifiedDateTime;
					role.ModifiedUser = obj.ModifiedUser;
					role.ModifiedDateTime = obj.ModifiedDateTime;
					int roleId = await cn.InsertAsync(role, tran);
					role.Id = roleId;
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

	public async Task<List<SystemModule>> GetListAsync(List<int> systemModuleIdList)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id IN @IdList");
		param.Add("@IdList", systemModuleIdList);

		using var cn = ConnectionFactory.GetDbConnection()!;

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		return (await cn.QueryAsync<SystemModule>(sql, param)).AsList();
	}

	public async Task<List<SystemModule>> SearchAsync(int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		bool? isEnabled = null,
		string? parentHierarchyPath = null,
		string? modulePath = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
		{
			sbSql.Where("t.ObjectCode=@ObjectCode");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(objectName))
		{
			sbSql.Where("t.ObjectName=@ObjectName");
			param.Add("@ObjectName", objectName, DbType.AnsiString);
		}

		if (isEnabled != null)
		{
			sbSql.Where("t.IsEnabled=@IsEnabled");
			param.Add("@IsEnabled", isEnabled);
		}

		if (!string.IsNullOrEmpty(parentHierarchyPath))
		{
			sbSql.Where("t.HierarchyPath LIKE @ParentHierarchyPath+'>%'");
			param.Add("@ParentHierarchyPath", parentHierarchyPath, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(modulePath))
		{
			sbSql.Where("t.ModulePath LIKE '%'+@ModulePath+'%'");
			param.Add("@ModulePath", modulePath, DbType.AnsiString);
		}
		#endregion

		sbSql.OrderBy("t.ObjectName ASC");

		sbSql.LeftJoin($"{DbObject.MsSqlTable} pr ON pr.Id=t.ParentId");

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				  $"SELECT t.*, pr.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = (await cn.QueryAsync<SystemModule, SystemModule, SystemModule>(sql,
															(obj, parent) => {
																obj.Parent = parent;
																return obj;
															}, param, splitOn: "Id")).AsList();

		return dataList;
	}

	public async Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		bool? isEnabled = null,
		string? parentHierarchyPath = null,
		string? modulePath = null)
	{
		if (pgSize < 0)
			throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
		{
			sbSql.Where("t.ObjectCode=@ObjectCode");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(objectName))
		{
			sbSql.Where("t.ObjectName=@ObjectName");
			param.Add("@ObjectName", objectName, DbType.AnsiString);
		}

		if (isEnabled != null)
		{
			sbSql.Where("t.IsEnabled=@IsEnabled");
			param.Add("@IsEnabled", isEnabled);
		}

		if (!string.IsNullOrEmpty(parentHierarchyPath))
		{
			sbSql.Where("t.HierarchyPath LIKE @ParentHierarchyPath+'>%'");
			param.Add("@ParentHierarchyPath", parentHierarchyPath, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(modulePath))
		{
			sbSql.Where("t.ModulePath LIKE '%'+@ModulePath+'%'");
			param.Add("@ModulePath", modulePath, DbType.AnsiString);
		}
		#endregion

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		using var cn = ConnectionFactory.GetDbConnection()!;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

		DataPagination pagination = new()
		{
			ObjectType = typeof(Address).Name,
			PageSize = pgSize,
			PageCount = pageCount,
			RecordCount = (int)recordCount
		};

		return pagination;
	}

	public override async Task<List<SystemModule>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		sbSql.Where("t.IsDeleted=0");

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

		sbSql.OrderBy("t.ObjectName ASC");

		sbSql.LeftJoin($"{DbObject.MsSqlTable} pr ON pr.Id=t.ParentId");

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT t.*,pr.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = (await cn.QueryAsync<SystemModule, SystemModule, SystemModule>(sql,
															(obj, parent) => {
																obj.Parent = parent;
																return obj;
															}, param, splitOn: "Id")).AsList();

		return dataList;
	}

	public async Task<List<DropDownListItem>> GetValidParentsAsync(int objectId, string objectCode, string hierarchyPath)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Select("'ObjectId'=t.Id")
			.Select("t.ObjectCode")
			.Select("t.ObjectName")
			.Select("t.HierarchyPath");

		sbSql.Where("t.IsDeleted=0")
			.Where("t.IsMenuGroup=1")
			.Where("t.Id<>@Id")
			.Where("t.ObjectCode<>@ObjectCode")
			.Where("t.HierarchyPath NOT LIKE @HierarchyPath+'>%'");

		param.Add("@Id", objectId);
		param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);

		using var cn = ConnectionFactory.GetDbConnection()!;

		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		List<DropDownListItem> dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();

		return dataList;
	}

	public async Task<List<DropDownListItem>> GetForDropdownAsync(int pgSize = 0, int pgNo = 0, string? searchText = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		sbSql.Where("t.IsDeleted=0");

		if (!string.IsNullOrEmpty(searchText))
		{
			sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
			param.Add("@SearchText", searchText, DbType.AnsiString);
		}
		sbSql.Select("'ObjectId'=t.Id")
			.Select("t.ObjectCode")
			.Select("t.ObjectName");

		sbSql.OrderBy("t.ObjectCode ASC")
			.OrderBy("t.ObjectName ASC");

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT /**select**/ FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		List<DropDownListItem> dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();

		return dataList;
	}

	public async Task<List<RoleSysMod>> GetByRoleAsync(int roleId)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		sbSql.LeftJoin($"{DbObject.MsSqlTable} sm ON sm.Id=t.SystemModuleId");
		sbSql.Where("t.RoleId=@RoleId");
		param.Add("@RoleId", roleId);

		using var cn = ConnectionFactory.GetDbConnection()!;
		string sql = sbSql.AddTemplate($"SELECT * FROM {RoleSysMod.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
		List<RoleSysMod> dataList = (await cn.QueryAsync<RoleSysMod, SystemModule, RoleSysMod>(sql, (obj, sysMod) =>
		{
			obj.SystemModule = sysMod;
			return obj;
		}, param)).AsList();

		return dataList;
	}

	public async Task<List<DropdownSelectItem>> GetAssignedObjectClassesAsync(string? excludeClassName = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Select("t.Id")
			.Select("'Key'=t.ObjectClassFullName")
			.Select("'Value'=t.ObjectClassName");

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("LEN(ISNULL(t.ObjectClassFullName,''))>0");

		if (!string.IsNullOrEmpty(excludeClassName))
		{
			sbSql.Where("t.ObjectClassFullName<>@ObjectClassFullName");
			param.Add("@ObjectClassFullName", excludeClassName, DbType.AnsiString);
		}

		using var cn = ConnectionFactory.GetDbConnection()!;
		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		var dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();
		return dataList;
	}

	public async Task<DataResult<SystemModule>> GetForSelectMenuAsync(int pgSize = 0, int pgNo = 0,
		string? searchText = null,
		List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.IsEnabled=1");
		sbSql.LeftJoin($"{DbObject.MsSqlTable} pr ON pr.Id=t.ParentId");

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
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				  $"SELECT t.*, pr.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		DataResult<SystemModule> dataResult = new();

		dataResult.Records = (await cn.QueryAsync<SystemModule, SystemModule, SystemModule>(sql,
													(obj, parent) => {
														obj.Parent = parent;
														return obj;
													}, param)).AsList();

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

		dataResult.Pagination = new()
		{
			ObjectType = typeof(SystemModule).Name,
			PageSize = pgSize,
			PageCount = pageCount,
			RecordCount = (int)recordCount
		};

		return dataResult;
	}
}