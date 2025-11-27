using PruFin=DataLayer.Models.Pru.Finance;
using static Dapper.SqlMapper;
using DataLayer.Models.Pru.Finance;

namespace DataLayer.Repos.Pru.Finance;

public interface IInvoiceRepos : IBaseRepos<PruFin.Invoice>
{
	Task<KeyValuePair<int, IEnumerable<PruFin.Invoice>>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		List<SqlSortCond>? sortConds = null,
		List<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);

	Task<PruFin.Invoice?> GetFullAsync(int id);

	Task<int> InsertOrUpdateFullAsync(PruFin.Invoice obj);
}

public class InvoiceRepos(IDbContext dbContext) : BaseRepos<PruFin.Invoice>(dbContext, PruFin.Invoice.DatabaseObject), IInvoiceRepos
{
	public async Task<KeyValuePair<int, IEnumerable<PruFin.Invoice>>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		List<SqlSortCond>? sortConds = null,
		List<SqlFilterCond>? filterConds = null,
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
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(v.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.AccSysSubmID) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		sbSql.LeftJoin($"{Vendor.MsSqlTableName} v ON v.IsDeleted=0 AND v.LBU=t.LBU AND v.ObjectCode=t.VendorID");

		if (sortConds != null && sortConds.Count != 0)
		{
			foreach (SqlSortCond sortCond in sortConds)
			{
				if (sortCond.FieldName == "VendorName")
					sbSql.OrderBy("v.ObjectName " + (sortCond.IsDecending ? "DESC" : "ASC"));
				else
					sbSql.OrderBy(sortCond.GetSortCommand("t"));
			}
		}
		else
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

		var dataList = (await cn.QueryAsync<PruFin.Invoice, Vendor, PruFin.Invoice>(sql, 
				(q, v) =>
				{
					q.Vendor = v;
					return q;
				}, param)).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<PruFin.Invoice>>(count, dataList);
	}

	public async Task<PruFin.Invoice?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");
		param.Add("@Id", id);

		//sbSql.LeftJoin($"{Vendor.MsSqlTable} v ON v.IsDeleted=0 AND v.ObjectCode=t.VendorID");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		var obj = await cn.QuerySingleOrDefaultAsync<PruFin.Invoice?>(sql, param);

		if (obj != null)
		{
			if (!string.IsNullOrEmpty(obj.VendorID))
			{
				obj.Vendor = await cn.QuerySingleOrDefaultAsync<Vendor?>($"SELECT * FROM {Vendor.MsSqlTable} v WHERE v.IsDeleted=0 AND v.LBU=@LBU AND v.ObjectCode=@VendorID", new { obj.VendorID, obj.LBU });
			}

			SqlBuilder sbSqlItem = new();
			DynamicParameters paramItem = new();
			sbSqlItem.Where("i.IsDeleted=0");
			sbSqlItem.Where("i.InvoiceId=@InvoiceId");
			paramItem.Add("@InvoiceId", obj.Id);

			string sqlItem = sbSqlItem.AddTemplate($"SELECT * FROM {PruFin.InvoiceItem.MsSqlTable} i /**where**/").RawSql;
			obj.Items = (await cn.QueryAsync<PruFin.InvoiceItem>(sqlItem, paramItem)).AsList();

			SqlBuilder sbSqlExpItem = new();
			sbSqlExpItem.Where("e.IsDeleted=0");
			sbSqlExpItem.Where("e.InvoiceId=@InvoiceId");

			string sqlExpItem = sbSqlExpItem.AddTemplate($"SELECT * FROM {ExpenseItem.MsSqlTable} e /**where**/").RawSql;
			obj.ExpenseItems = (await cn.QueryAsync<ExpenseItem>(sqlExpItem, paramItem)).AsList();
		}

		return obj;
	}


	public async Task<int> InsertOrUpdateFullAsync(PruFin.Invoice obj)
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
					foreach (PruFin.InvoiceItem item in obj.Items)
					{
						if (item.Id > 0)
						{
							item.InvoiceId = obj.Id;
							item.InvoiceCode = obj.ObjectCode;
							item.ModifiedUser = obj.ModifiedUser;
							item.ModifiedDateTime = obj.ModifiedDateTime;
							bool isItemUpdated = await cn.UpdateAsync(item, tran);
						}
						else
						{
							if (item.IsDeleted) continue;

							item.InvoiceId = obj.Id;
							item.InvoiceCode = obj.ObjectCode;
							item.CreatedUser = obj.CreatedUser;
							item.CreatedDateTime = obj.CreatedDateTime;
							item.ModifiedUser = obj.ModifiedUser;
							item.ModifiedDateTime = obj.ModifiedDateTime;
							int itemId = await cn.InsertAsync(item, tran);
						}
					}

					foreach (ExpenseItem expItem in obj.ExpenseItems)
					{
						if (expItem.Id > 0)
						{
							expItem.InvoiceId = obj.Id;
							expItem.InvoiceCode = obj.ObjectCode;
							expItem.InvoiceDate = obj.EffectiveDate;
							expItem.LBU = obj.LBU;
							expItem.ModifiedUser = obj.ModifiedUser;
							expItem.ModifiedDateTime = obj.ModifiedDateTime;
							bool isExpItemUpdated = await cn.UpdateAsync(expItem, tran);
						}
						else
						{
							if (expItem.IsDeleted) continue;

							expItem.InvoiceId = obj.Id;
							expItem.InvoiceCode = obj.ObjectCode;
							expItem.InvoiceDate = obj.EffectiveDate;
							expItem.AccSysID = obj.AccSysSubmID;
							expItem.VendorID = obj.VendorID;
							expItem.LBU = obj.LBU;
							expItem.CreatedUser = obj.CreatedUser;
							expItem.CreatedDateTime = obj.CreatedDateTime;
							expItem.ModifiedUser = obj.ModifiedUser;
							expItem.ModifiedDateTime = obj.ModifiedDateTime;
							int expItemId = await cn.InsertAsync(expItem, tran);
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
					foreach (PruFin.InvoiceItem item in obj.Items)
					{
						if (item.IsDeleted) continue;

						item.InvoiceId = objId;
						item.InvoiceCode = obj.ObjectCode;
						item.CreatedUser = obj.CreatedUser;
						item.CreatedDateTime = obj.CreatedDateTime;
						item.ModifiedUser = obj.ModifiedUser;
						item.ModifiedDateTime = obj.ModifiedDateTime;

						int itemId = await cn.InsertAsync(item, tran);
					}

					foreach (ExpenseItem expItem in obj.ExpenseItems)
					{
						if (expItem.IsDeleted) continue;

						expItem.InvoiceId = objId;
						expItem.InvoiceCode = obj.ObjectCode;
						expItem.AccSysID = obj.AccSysSubmID;
						expItem.VendorID = obj.VendorID;
						expItem.LBU = obj.LBU;
						expItem.CreatedUser = obj.CreatedUser;
						expItem.CreatedDateTime = obj.CreatedDateTime;
						expItem.ModifiedUser = obj.ModifiedUser;
						expItem.ModifiedDateTime = obj.ModifiedDateTime;

						int expItemId = await cn.InsertAsync(expItem, tran);
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

	public override List<string> GetSearchOrderbBy()
	{
		return ["t.EffectiveDate DESC", "t.ObjectName ASC"];
	}
}