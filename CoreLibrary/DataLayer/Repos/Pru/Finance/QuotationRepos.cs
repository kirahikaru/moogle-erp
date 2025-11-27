using DataLayer.GlobalConstant.Pru;
using DataLayer.Models.Pru.Finance;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Pru.Finance;

public interface IQuotationRepos : IBaseRepos<Quotation>
{
	Task<KeyValuePair<int, IEnumerable<Quotation>>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);


	Task<Quotation?> GetFullAsync(int id);
	Task<int> InsertOrUpdateFullAsync(Quotation obj);
	Task<IEnumerable<Quotation>> GetForPRPOAsync();
}

public class QuotationRepos(IDbContext dbContext) : BaseRepos<Quotation>(dbContext, Quotation.DatabaseObject), IQuotationRepos
{
	public async Task<Quotation?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");
		param.Add("@Id", id);

		//sbSql.LeftJoin($"{Vendor.MsSqlTable} v ON v.IsDeleted=0 AND v.ObjectCode=t.VendorID");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		var obj = await cn.QuerySingleOrDefaultAsync<Quotation?>(sql, param);

		if (obj != null)
		{
			if (!string.IsNullOrEmpty(obj.VendorID))
			{
				obj.Vendor = await cn.QuerySingleOrDefaultAsync<Vendor?>($"SELECT * FROM {Vendor.MsSqlTable} v WHERE v.IsDeleted=0 AND v.LBU=@LBU AND v.ObjectCode=@VendorID", new { obj.LBU, obj.VendorID });
			}

			SqlBuilder sbSqlItem = new();
			DynamicParameters paramItem = new();
			sbSqlItem.Where("qi.IsDeleted=0");
			sbSqlItem.Where("qi.QuotationId=@QuotationId");
			paramItem.Add("@QuotationId", obj.Id);

			string sqlItem = sbSqlItem.AddTemplate($"SELECT * FROM {QuotationItem.MsSqlTable} qi /**where**/").RawSql;
			obj.Items = (await cn.QueryAsync<QuotationItem>(sqlItem, paramItem)).AsList();
		}

		return obj;
	}

	public async Task<KeyValuePair<int, IEnumerable<Quotation>>> SearchAsync(
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
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(v.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		sbSql.LeftJoin($"{Vendor.MsSqlTable} v ON v.IsDeleted=0 AND v.ObjectCode=t.VendorID");

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
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<Quotation, Vendor, Quotation>(sql, 
				(q, v) =>
				{
					q.Vendor = v;
					return q;
				}, param, splitOn:"Id")).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<Quotation>>(count, dataList);
	}

	public async Task<int> InsertOrUpdateFullAsync(Quotation obj)
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

				if (isUpdated && obj.Items.Count > 0)
				{
					foreach (QuotationItem item in obj.Items)
					{
						if (item.Id > 0)
						{
							item.QuotationId = obj.Id;
							item.QuotationCode = obj.ObjectCode;
							item.ModifiedUser = obj.ModifiedUser;
							item.ModifiedDateTime = obj.ModifiedDateTime;
							bool isItemUpdated = await cn.UpdateAsync(item, tran);
						}
						else
						{
							if (item.IsDeleted) continue;

							item.QuotationId = obj.Id;
							item.QuotationCode = obj.ObjectCode;
							item.CreatedUser = obj.CreatedUser;
							item.CreatedDateTime = obj.CreatedDateTime;
							item.ModifiedUser = obj.ModifiedUser;
							item.ModifiedDateTime = obj.ModifiedDateTime;
							int itemId = await cn.InsertAsync(item, tran);
						}
					}
				}
			}
			else
			{
				int objId = await cn.InsertAsync(obj, tran);

				if (objId > 0 && obj.Items.Count > 0)
				{
					obj.Id = obj.Id;
					foreach (QuotationItem item in obj.Items)
					{
						if (item.IsDeleted) continue;

						item.QuotationId = objId;
						item.QuotationCode = obj.ObjectCode;
						item.CreatedUser = obj.CreatedUser;
						item.CreatedDateTime = obj.CreatedDateTime;
						item.ModifiedUser = obj.ModifiedUser;
						item.ModifiedDateTime = obj.ModifiedDateTime;

						int itemId = await cn.InsertAsync(item, tran);
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

	public async Task<IEnumerable<Quotation>> GetForPRPOAsync()
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.WorkflowStatus=@WorkflowStatus");

		param.Add("@WorkflowStatus", QuotationWFStatuses.CONFIRMED, DbType.AnsiString);
		sbSql.OrderBy("t.EffectiveDate ASC");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		var dataList = await cn.QueryAsync<Quotation>(sql, param);
		return dataList;
	}

	public override List<string> GetSearchOrderbBy()
	{
		return ["t.EffectiveDate DESC", "t.ObjectName ASC"];
	}
}