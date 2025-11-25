namespace DataLayer.Repos.FIN;

public interface IBankRepos : IBaseRepos<Bank>
{
	Task<List<DropdownSelectItem>> GetForDropdownSelect1Async();

	Task<List<Bank>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? displayName = null,
		List<string>? bankTypeList = null,
		string? addressText = null,
		string? addressKhText = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? displayName = null,
		List<string>? bankTypeList = null,
		string? addressText = null,
		string? addressKhText = null);
}

public class BankRepos(IDbContext dbContext) : BaseRepos<Bank>(dbContext, Bank.DatabaseObject), IBankRepos
{
	public async Task<List<DropdownSelectItem>> GetForDropdownSelect1Async()
    {
        SqlBuilder sbSql = new();

        using var cn = DbContext.DbCxn;

        DynamicParameters param = new();

        sbSql.Select("t.Id");
        sbSql.Select("'Key'=t.ObjectCode");
        sbSql.Select("'Value'=t.DisplayName");
        sbSql.Select("'ValueKh'=t.ObjectNameKh");

        sbSql.Where("t.IsDeleted=0");

        sbSql.OrderBy("t.ObjectName ASC");

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        var dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return dataList;
    }

	public async Task<List<Bank>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? displayName = null,
		List<string>? bankTypeList = null,
		string? addressText = null,
		string? addressKhText = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();

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
			param.Add("@ObjectName", objectName, DbType.AnsiString);
		}

		if (bankTypeList != null && bankTypeList.Count != 0)
		{
			if (bankTypeList.Count == 1)
			{
				sbSql.Where("t.BankType=@BankType");
				param.Add("@BankType", bankTypeList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.BankType IN @BankTypeList");
				param.Add("@BankTypeList", bankTypeList);
			}
		}

		if (!string.IsNullOrEmpty(displayName))
		{
			sbSql.Where("t.DisplayName LIKE '%'+@DisplayName+'%'");
			param.Add("@DisplayName", displayName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(addressText))
		{
			sbSql.Where("UPPER(t.AddressText) LIKE '%'+UPPER(@AddressText)+'%'");
			param.Add("@AddressText", addressText, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(addressKhText))
		{
			sbSql.Where("UPPER(t.AddressKhText) LIKE '%'+UPPER(@AddressKhText)+'%'");
			param.Add("@AddressKhText", addressKhText, DbType.AnsiString);
		}
		#endregion

		sbSql.OrderBy("t.ObjectName ASC");
		sbSql.OrderBy("t.ObjectNameKh ASC");

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
				$"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg z ON z.Id=t.Id /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<Bank>(sql, param)).AsList();

		return dataList;
	}

	public async Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? displayName = null,
		List<string>? bankTypeList = null,
		string? addressText = null,
		string? addressKhText = null)
	{
		if (pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();

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
			param.Add("@ObjectName", objectName, DbType.AnsiString);
		}

		if (bankTypeList != null && bankTypeList.Count != 0)
		{
			if (bankTypeList.Count == 1)
			{
				sbSql.Where("t.BankType=@BankType");
				param.Add("@BankType", bankTypeList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.BankType IN @BankTypeList");
				param.Add("@BankTypeList", bankTypeList);
			}
		}

		if (!string.IsNullOrEmpty(displayName))
		{
			sbSql.Where("t.DisplayName LIKE '%'+@DisplayName+'%'");
			param.Add("@DisplayName", displayName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(addressText))
		{
			sbSql.Where("UPPER(t.AddressText) LIKE '%'+UPPER(@AddressText)+'%'");
			param.Add("@AddressText", addressText, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(addressKhText))
		{
			sbSql.Where("UPPER(t.AddressKhText) LIKE '%'+UPPER(@AddressKhText)+'%'");
			param.Add("@AddressKhText", addressKhText, DbType.AnsiString);
		}
		#endregion

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		using var cn = DbContext.DbCxn;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

		DataPagination pagination = new()
		{
			ObjectType = typeof(Bank).Name,
			PageSize = pgSize,
			PageCount = pageCount,
			RecordCount = (int)recordCount
		};

		return pagination;
	}
}