namespace DataLayer.Repos.FIN;

using Dapper.Contrib.Extensions;

public interface IExchangeRateRepos : IBaseRepos<ExchangeRate>
{
	Task<ExchangeRate?> GetRateAsync(string fromCurrencyCode, string toCurrencyCode, DateTime date);
	Task<ExchangeRate?> GetCurrentAsync(string fromCurrencyCode, string toCurrencyCode);
	Task<List<ExchangeRate>> GetHistoryAsync(string fromCurrencyCode, string toCurrencyCode);
	Task<int> CommitAndProcessAsync(ExchangeRate obj);
	Task<int> UpdateExchangeRateAsync(int currentObjId, ExchangeRate newExchangeRate);
}

public class ExchangeRateRepos(IDbContext dbContext) : BaseRepos<ExchangeRate>(dbContext, ExchangeRate.DatabaseObject), IExchangeRateRepos
{
	public async Task<ExchangeRate?> GetRateAsync(string fromCurrencyCode, string toCurrencyCode, DateTime date)
    {
        var sql = $"SELECT * FROM {ExchangeRate.MsSqlTable} WHERE IsDeleted=0 AND FromCurrencyCode=@FromCurrencyCode AND ToCurrencyCode=@ToCurrencyCode AND StartDate<=@StartDate AND (EndDate IS NULL OR EndDate<=@EndDate)";
        DynamicParameters param = new();

        param.Add("@FromCurrencyCode", fromCurrencyCode, DbType.AnsiString);
        param.Add("@ToCurrencyCode", toCurrencyCode, DbType.AnsiString);
        param.Add("@StartDate", date);
        param.Add("EndDate", date);

        using var cn = DbContext.DbCxn;

        var data = await cn.QueryFirstOrDefaultAsync<ExchangeRate>(sql, param);

        return data;
    }

	public override async Task<KeyValuePair<int, IEnumerable<ExchangeRate>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0, string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

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

		if (sortConds != null && sortConds.Any())
		{
			foreach (var sortCond in sortConds)
				sbSql.OrderBy(sortCond.GetSortCommand("t"));
		}
		else
		{
			foreach (string orderBy in GetSearchOrderbBy())
				sbSql.OrderBy(orderBy);
		}

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);
			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<ExchangeRate>(sql, param);

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public override List<string> GetSearchOrderbBy()
    {
        return ["t.ObjectCode ASC", "t.StartDate DESC"];
    }

    public async Task<ExchangeRate?> GetCurrentAsync(string fromCurrencyCode, string toCurrencyCode)
    {
        var sql = $"SELECT * FROM {ExchangeRate.MsSqlTable} WHERE IsDeleted=0 AND FromCurrencyCode=@FromCurrencyCode AND ToCurrencyCode=@ToCurrencyCode AND IsCurrent=1";

        DynamicParameters param = new();

        param.Add("@FromCurrencyCode", fromCurrencyCode, DbType.AnsiString);
        param.Add("@ToCurrencyCode", toCurrencyCode, DbType.AnsiString);

        using var cn = DbContext.DbCxn;

        var data = await cn.QueryFirstOrDefaultAsync<ExchangeRate>(sql, param);

        return data;
    }

    public async Task<List<ExchangeRate>> GetHistoryAsync(string fromCurrencyCode, string toCurrencyCode)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.FromCurrencyCode=@FromCurrencyCode");
        sbSql.Where("t.ToCurrencyCode=@ToCurrencyCode");
        sbSql.OrderBy("t.StartDate DESC");
        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

        DynamicParameters param = new();
        param.Add("@FromCurrencyCode", fromCurrencyCode, DbType.AnsiString);
        param.Add("@ToCurrencyCode", toCurrencyCode, DbType.AnsiString);

        using var cn = DbContext.DbCxn;
        var dataList = (await cn.QueryAsync<ExchangeRate>(sql, param)).AsList();

        return dataList;
    }

    public async Task<int> CommitAndProcessAsync(ExchangeRate obj)
    {
        using var cn = DbContext.DbCxn;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();
        using var tran = cn.BeginTransaction();
        try
        {
            SqlBuilder sbCmdUpdCurrRate = new();
            sbCmdUpdCurrRate.Where("IsDeleted=0");
            sbCmdUpdCurrRate.Where("FromCurrencyCode=@FromCurrencyCode");
            sbCmdUpdCurrRate.Where("ToCurrencyCode=@ToCurrencyCode");
            sbCmdUpdCurrRate.Where("(IsCurrent=1 OR (EndDate IS NOT NULL AND EndDate>@StartDate))");

            sbCmdUpdCurrRate.Set("EndDate=@StartDate");
            sbCmdUpdCurrRate.Set("IsCurrent=0");
            sbCmdUpdCurrRate.Set("ModifiedUser=@ModifiedUser");
            sbCmdUpdCurrRate.Set("ModifiedDateTime=@ModifiedDateTime");

            string cmdUpdCurrRate = sbCmdUpdCurrRate.AddTemplate($"UPDATE {ExchangeRate.MsSqlTable} /**set**/ /**where**/").RawSql;
            DynamicParameters cmdUpdCurrRateParam = new();
            cmdUpdCurrRateParam.Add("@StartDate", obj.StartDate!.Value);
            cmdUpdCurrRateParam.Add("@FromCurrencyCode", obj.FromCurrencyCode!, DbType.AnsiString);
            cmdUpdCurrRateParam.Add("@ToCurrencyCode", obj.ToCurrencyCode!, DbType.AnsiString);
            cmdUpdCurrRateParam.Add("@ModifiedUser", obj.ModifiedUser!);
            cmdUpdCurrRateParam.Add("@ModifiedDateTime", obj.ModifiedDateTime!);

            await cn.ExecuteAsync(cmdUpdCurrRate, cmdUpdCurrRateParam, tran);

            int objId = -1;

            if (obj.Id == 0)
            {
                obj.IsCurrent = true;
                objId = await cn.InsertAsync(obj, tran);
            }
            else
            {
                objId = await cn.UpdateAsync(obj, tran) ? 1 : 0;
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
    public async Task<int> UpdateExchangeRateAsync(int currentObjId, ExchangeRate newExchangeRate)
    {
        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

        var updSql = $"UPDATE {ExchangeRate.MsSqlTable} SET EndDate=@EndDate, IsCurrent=0, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime WHERE Id=@Id";
        var updParam = new { EndDate = newExchangeRate.StartDate!.Value.AddDays(-1), ModifiedUser = newExchangeRate.CreatedUser, ModifiedDateTime = khTimestamp, Id = currentObjId };

        using var cn = DbContext.DbCxn;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();
        using var tran = cn.BeginTransaction();
        try
        {
            int updCount = await cn.ExecuteAsync(updSql, updParam, tran);

            if (updCount <= 0)
                throw new Exception("failed to update current exchange rate.");

            newExchangeRate.CreatedDateTime = khTimestamp;
            newExchangeRate.ModifiedDateTime = khTimestamp;
            int objId = await cn.InsertAsync(newExchangeRate);

            tran.Commit();
            return objId;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }
}