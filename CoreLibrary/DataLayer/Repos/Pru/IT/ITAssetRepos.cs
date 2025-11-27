using DataLayer.Models.Pru.IT;
using PruHR=DataLayer.Models.Pru.HR;
using static Dapper.SqlMapper;
using DataLayer.GlobalConstant.Pru;

namespace DataLayer.Repos.Pru.IT;

public interface IITAssetRepos : IBaseRepos<ITAsset>
{
	Task<ITAsset?> GetFullAsync(int id);

	Task<int> InsertOrUpdateFullAsync(ITAsset obj);

	Task<IEnumerable<ITAsset>> GetByEmployeeAsync(string empId);

	Task<KeyValuePair<int, IEnumerable<ITAsset>>> HardwareSearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);

	Task<KeyValuePair<int, IEnumerable<ITAsset>>> SoftwareSearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);

	Task<KeyValuePair<int, IEnumerable<ITAsset>>> CloudSearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);
}

public class ITAssetRepos(IDbContext dbContext) : BaseRepos<ITAsset>(dbContext, ITAsset.DatabaseObject), IITAssetRepos
{
	public async Task<ITAsset?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");
		param.Add("@Id", id);

		sbSql.LeftJoin($"{Employee.MsSqlTable} reqEmp ON reqEmp.EmpID=t.RequestUserID");
		sbSql.LeftJoin($"{Employee.MsSqlTable} curUsr ON curUsr.EmpID=t.CurrentUserID");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		var dataList = (await cn.QueryAsync<ITAsset, PruHR.Employee, PruHR.Employee, ITAsset>(sql, (obj, requestor, currentUser) =>
		{
			obj.Requestor = requestor;
			obj.CurrentUser = currentUser;
			return obj;

		}, param, splitOn: "Id")).AsList();

		if (dataList.Any())
		{
			SqlBuilder sbSqlItem = new();
			DynamicParameters paramItem = new();
			sbSqlItem.Where("t.IsDeleted=0");
			sbSqlItem.Where("t.AssetId=@AssetId");
			paramItem.Add("@AssetId", id);
			sbSqlItem.OrderBy("t.RequestDate").OrderBy("t.EffectiveDate");
			string sqlItem = sbSqlItem.AddTemplate($"SELECT * FROM {ITAssetAuditTrail.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
			dataList[0].AuditTrails = (await cn.QueryAsync<ITAssetAuditTrail>(sqlItem, paramItem)).AsList();

			if (dataList[0].CategoryCode == "SERVER")
			{
				string sqlServerInfo = $"SELECT * FROM {ITAssetServerInfo.MsSqlTable} WHERE IsDeleted=0 AND ITAssetId=@ITAssetId";
				dataList[0].ServerInfo = await cn.QuerySingleOrDefaultAsync<ITAssetServerInfo?>(sqlServerInfo, new { ITAssetId = dataList[0].Id });
			}

			return dataList[0];
		}

		return null;
	}

	public async Task<IEnumerable<ITAsset>> GetByEmployeeAsync(string empId)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.CurrentUserID=@EmpID");
		param.Add("@EmpID", empId, DbType.AnsiString);

		using var cn = DbContext.DbCxn;

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		var dataList = await cn.QueryAsync<ITAsset>(sql, param);

		return dataList;
	}


	public async Task<KeyValuePair<int, IEnumerable<ITAsset>>> HardwareSearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("UPPER(ISNULL(t.AssetType,''))=@AssetType");
		param.Add("@AssetType", AssetTypes.Hardware.ToUpper(), DbType.AnsiString);

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
			else if (searchText.StartsWith("sr:"))
			{
				sbSql.Where("UPPER(t.SerialNo) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("sr:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.SerialNo) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.CurrentUserName) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		foreach (string orderByClause in GetSearchOrderbBy())
			sbSql.OrderBy(orderByClause);

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
				$"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using IDbConnection cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<ITAsset>(sql, param)).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<ITAsset>>(count, dataList);
	}

	public async Task<KeyValuePair<int, IEnumerable<ITAsset>>> SoftwareSearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("UPPER(ISNULL(t.AssetType,''))=@AssetType");
		param.Add("@AssetType", AssetTypes.Software.ToUpper(), DbType.AnsiString);

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
			else if (searchText.StartsWith("sr:"))
			{
				sbSql.Where("UPPER(t.SerialNo) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("sr:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.SerialNo) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.CurrentUserName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.CurrentUserDept) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		foreach (string orderByClause in GetSearchOrderbBy())
			sbSql.OrderBy(orderByClause);

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
				$"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using IDbConnection cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<ITAsset>(sql, param)).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<ITAsset>>(count, dataList);
	}

	public async Task<KeyValuePair<int, IEnumerable<ITAsset>>> CloudSearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("UPPER(ISNULL(t.AssetType,''))=@AssetType");
		param.Add("@AssetType", AssetTypes.CloudInfra.ToUpper(), DbType.AnsiString);

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
			else if (searchText.StartsWith("sr:"))
			{
				sbSql.Where("UPPER(t.SerialNo) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("sr:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.SerialNo) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.CurrentUserName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.CurrentUserDept) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		foreach (string orderByClause in GetSearchOrderbBy())
			sbSql.OrderBy(orderByClause);

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
				$"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using IDbConnection cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<ITAsset>(sql, param)).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<ITAsset>>(count, dataList);
	}

	public override List<string> GetSearchOrderbBy()
	{
		return ["t.LifeCycleStatus ASC, t.CurrentUserDept ASC, t.CurrentUserFunc ASC, t.ObjectName ASC"];
	}


	public async Task<int> InsertOrUpdateFullAsync(ITAsset obj)
	{
		using var cn = DbContext.DbCxn;
		if (cn.State != ConnectionState.Open)
			cn.Open();

		using var tran = cn.BeginTransaction();

		try
		{
			if (obj.Id > 0) // Update
			{
				bool isUpdated = await cn.UpdateAsync(obj, tran);

				if (isUpdated)
				{
					if (obj.AuditTrails.Count > 0)
					{
						foreach (ITAssetAuditTrail item in obj.AuditTrails)
						{
							if (item.Id > 0)
							{
								item.AssetId = obj.Id;
								item.AssetCode = obj.ObjectCode;
								item.ModifiedUser = obj.ModifiedUser;
								item.ModifiedDateTime = obj.ModifiedDateTime;
								bool isItemUpdated = await cn.UpdateAsync(item, tran);
							}
							else
							{
								if (item.IsDeleted) continue;

								item.AssetId = obj.Id;
								item.AssetCode = obj.ObjectCode;
								item.CreatedUser = obj.CreatedUser;
								item.CreatedDateTime = obj.CreatedDateTime;
								item.ModifiedUser = obj.ModifiedUser;
								item.ModifiedDateTime = obj.ModifiedDateTime;
								int itemId = await cn.InsertAsync(item, tran);
							}
						}
					}

					if (obj.ServerInfo != null)
					{
						if (obj.ServerInfo.Id == 0)
						{
							obj.ServerInfo.ObjectCode = obj.ObjectCode;
							obj.ServerInfo.ITAssetId = obj.Id;
							obj.ServerInfo.CreatedUser = obj.ModifiedUser;
							obj.ServerInfo.CreatedDateTime = obj.ModifiedDateTime;
							obj.ServerInfo.ModifiedUser = obj.ModifiedUser;
							obj.ServerInfo.ModifiedDateTime = obj.ModifiedDateTime;

							int serverInfoId = await cn.InsertAsync(obj.ServerInfo, tran);
						}
						else
						{
							obj.ServerInfo.ObjectCode = obj.ObjectCode;
							obj.ServerInfo.ITAssetId = obj.Id;
							obj.ServerInfo.ModifiedUser = obj.ModifiedUser;
							obj.ServerInfo.ModifiedDateTime = obj.ModifiedDateTime;

							bool isServerInfoUpd = await cn.UpdateAsync(obj.ServerInfo, tran);
						}
					}
				}
			}
			else
			{
				int objId = await cn.InsertAsync(obj, tran);

				if (objId > 0)
				{
					obj.Id = obj.Id;
					if (obj.AuditTrails.Count > 0)
					{
						foreach (ITAssetAuditTrail item in obj.AuditTrails)
						{
							if (item.IsDeleted) continue;

							item.AssetId = objId;
							item.AssetCode = obj.ObjectCode;
							item.CreatedUser = obj.CreatedUser;
							item.CreatedDateTime = obj.CreatedDateTime;
							item.ModifiedUser = obj.ModifiedUser;
							item.ModifiedDateTime = obj.ModifiedDateTime;

							int itemId = await cn.InsertAsync(item, tran);
						}
					}

					if (obj.ServerInfo != null)
					{
						obj.ServerInfo.ObjectCode = obj.ObjectCode;
						obj.ServerInfo.ITAssetId = obj.Id;
						obj.ServerInfo.CreatedUser = obj.ModifiedUser;
						obj.ServerInfo.CreatedDateTime = obj.ModifiedDateTime;
						obj.ServerInfo.ModifiedUser = obj.ModifiedUser;
						obj.ServerInfo.ModifiedDateTime = obj.ModifiedDateTime;

						int serverInfoId = await cn.InsertAsync(obj.ServerInfo, tran);
					}
				}
			}
			tran.Commit();
			return obj.Id;
		}
		catch
		{
			tran.Rollback();
			throw;
		}
	}
}