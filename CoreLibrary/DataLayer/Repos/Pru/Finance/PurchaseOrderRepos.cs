using PruFin=DataLayer.Models.Pru.Finance;
using static Dapper.SqlMapper;
using DataLayer.Models.Pru.Finance;
using DataLayer.GlobalConstant.Pru;

namespace DataLayer.Repos.Pru.Finance;

public interface IPurchaseOrderRepos : IBaseRepos<PruFin.PurchaseOrder>
{
	Task<KeyValuePair<int, IEnumerable<PruFin.PurchaseOrder>>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);

	Task<PruFin.PurchaseOrder?> GetFullAsync(int id);

	Task<int> InsertOrUpdateFullAsync(PruFin.PurchaseOrder obj);
}

public class PurchaseOrderRepos(IDbContext dbContext) : BaseRepos<PruFin.PurchaseOrder>(dbContext, PruFin.PurchaseOrder.DatabaseObject), IPurchaseOrderRepos
{
	public async Task<KeyValuePair<int, IEnumerable<PruFin.PurchaseOrder>>> SearchAsync(
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
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.PRID) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(v.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		sbSql.LeftJoin($"{PruFin.Vendor.MsSqlTable} v ON v.IsDeleted=0 AND v.LBU=t.LBU AND v.ObjectCode=t.VendorID");

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

		var dataList = (await cn.QueryAsync<PruFin.PurchaseOrder, PruFin.Vendor, PruFin.PurchaseOrder>(sql, 
				(q, v) =>
				{
					q.Vendor = v;
					return q;
				}, param)).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<PruFin.PurchaseOrder>>(count, dataList);
	}

	public async Task<PruFin.PurchaseOrder?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");
		param.Add("@Id", id);

		sbSql.LeftJoin($"{Vendor.MsSqlTable} v ON v.IsDeleted=0 AND v.LBU=t.LBU AND v.ObjectCode=t.VendorID");
		sbSql.LeftJoin($"{Quotation.MsSqlTable} q ON q.IsDeleted=0 AND q.LBU=t.LBU AND q.ObjectCode=t.QuotationCode");

		//sbSql.LeftJoin($"{Vendor.MsSqlTable} v ON v.IsDeleted=0 AND v.ObjectCode=t.VendorID");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
		var obj = (await cn.QueryAsync<PruFin.PurchaseOrder, Vendor, Quotation, PruFin.PurchaseOrder>(sql, (po, vendor, qnt) =>
		{
			po.Vendor = vendor;
			po.Quotation = qnt;

			return po;
		}, param, splitOn: "Id")).SingleOrDefault();

		if (obj != null)
		{
			SqlBuilder sbSqlItem = new();
			DynamicParameters paramItem = new();
			sbSqlItem.Where("qi.IsDeleted=0");
			sbSqlItem.Where("qi.PurchaseOrderId=@PurchaseOrderId");
			paramItem.Add("@PurchaseOrderId", obj.Id);

			string sqlItem = sbSqlItem.AddTemplate($"SELECT * FROM {PruFin.PurchaseOrderItem.MsSqlTable} qi /**where**/").RawSql;
			obj.Items = (await cn.QueryAsync<PruFin.PurchaseOrderItem>(sqlItem, paramItem)).AsList();
		}

		return obj;
	}


	public async Task<int> InsertOrUpdateFullAsync(PruFin.PurchaseOrder obj)
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
					foreach (PruFin.PurchaseOrderItem item in obj.Items)
					{
						if (item.Id > 0)
						{
							item.PurchaseOrderId = obj.Id;
							item.PurchaseOrderCode = obj.ObjectCode;
							item.ModifiedUser = obj.ModifiedUser;
							item.ModifiedDateTime = obj.ModifiedDateTime;
							bool isItemUpdated = await cn.UpdateAsync(item, tran);
						}
						else
						{
							if (item.IsDeleted) continue;

							item.PurchaseOrderId = obj.Id;
							item.PurchaseOrderCode = obj.ObjectCode;
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
					foreach (PruFin.PurchaseOrderItem item in obj.Items)
					{
						if (item.IsDeleted) continue;

						item.PurchaseOrderId = objId;
						item.PurchaseOrderCode = obj.ObjectCode;
						item.CreatedUser = obj.CreatedUser;
						item.CreatedDateTime = obj.CreatedDateTime;
						item.ModifiedUser = obj.ModifiedUser;
						item.ModifiedDateTime = obj.ModifiedDateTime;

						int itemId = await cn.InsertAsync(item, tran);
					}
				}
			}

			if (obj.Id > 0 && obj.WorkflowStatus.Is(PurchaseOrderWFStatuses.PR_RAISED, PurchaseOrderWFStatuses.PR_APPROVED) && obj.QuotationId.HasValue)
			{
				string sqlUpdQuot = $"UPDATE {Quotation.MsSqlTable} SET WorkflowStatus=@TargetWorkflowStatus, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime WHERE Id=@QuotationId AND WorkflowStatus=@CurrentWorkflowStatus";
				DynamicParameters updQuotParam = new();
				updQuotParam.Add("@TargetWorkflowStatus", QuotationWFStatuses.PO_RAISED);
				updQuotParam.Add("@QuotationId", obj.QuotationId!.Value);
				updQuotParam.Add("@ModifiedUser", obj.ModifiedUser);
				updQuotParam.Add("@ModifiedDateTime", obj.ModifiedDateTime);
				updQuotParam.Add("@CurrentWorkflowStatus", QuotationWFStatuses.CONFIRMED);
				int updQuotCount = await cn.ExecuteAsync(sqlUpdQuot, updQuotParam, tran);
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