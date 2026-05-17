namespace DataLayer.Repos.SysCore;

/// <summary>
/// Repository : Cambodia - Village
/// </summary>
public interface IKhVillageRepos : IBaseRepos<KhVillage>
{
	Task<KhVillage?> GetFullAsync(int id);

	Task<List<DropDownListItem>> GetForDropdownSelect1Async(int? khCommuneId = null, string? searchText = null);

	new Task<List<SearchItemKhVillage>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null);

	Task<List<SearchItemKhVillage>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameKh = null,
		string? nameEn = null,
		string? postalCode = null,
		List<int>? khCommuneIds = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameKh = null,
		string? nameEn = null,
		string? postalCode = null,
		List<int>? khCommuneIds = null);
}

public class KhVillageRepos(IDbContext dbContext) : BaseRepos<KhVillage>(dbContext, KhVillage.DatabaseObject), IKhVillageRepos
{
	public async Task<KhVillage?> GetFullAsync(int id)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");

        param.Add("@Id", id);

		sbSql.LeftJoin($"{KhCommune.MsSqlTable} c ON c.Id=t.KhCommuneId");
		sbSql.LeftJoin($"{KhDistrict.MsSqlTable} d ON d.Id=c.KhDistrictId");
		sbSql.LeftJoin($"{KhProvince.MsSqlTable} p ON p.Id=d.KhProvinceId");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
		var dataList = (await cn.QueryAsync<KhVillage, KhCommune, KhDistrict, KhProvince, KhVillage>(sql,
								(obj, c, d, p) =>
								{
									if (d != null)
										d.Province = p;

                                    if (c != null)
                                        c.District = d;

                                    obj.Commune = c;
									return obj;
								}, param, splitOn:"Id")).AsList();

        if (dataList != null && dataList.Count != 0)
			return dataList[0];
		else
			return null;
	}

	public async Task<List<DropDownListItem>> GetForDropdownSelect1Async(int? khCommuneId = null, string? searchText = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("'ObjectId'=t.Id");
        sbSql.Select("t.ObjectCode");
        sbSql.Select("t.ObjectName");
        sbSql.Select("'ObjectType'='VILLAGE'");
        sbSql.Select("'ObjectNameEn'=t.NameEn");
        sbSql.Select("'ObjectNameKh'=t.NameKh");

        sbSql.Where("t.IsDeleted=0");

        if (khCommuneId.HasValue)
        {
            sbSql.Where("t.KhCommuneId IS NOT NULL");
            sbSql.Where("t.KhCommuneId=@KhCommuneId");
            param.Add("@KhCommuneId", khCommuneId.Value);
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else
            {
                sbSql.Where("(UPPER(t.NameEn) LIKE '%'+UPPER(@SearchText)+'%' OR t.NameKh LIKE '%'+@SearchText+'%')");
            }

            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        var sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();
    }

	public override async Task<KeyValuePair<int, IEnumerable<KhVillage>>> SearchNewAsync(
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

		sbSql.LeftJoin($"{KhCommune.MsSqlTable} c ON c.Id=t.KhCommuneId");
		sbSql.LeftJoin($"{KhDistrict.MsSqlTable} d ON d.Id=c.KhDistrictId");
		sbSql.LeftJoin($"{KhProvince.MsSqlTable} prv ON prv.Id=d.KhProvinceId");
		
		sbSql.OrderBy("t.ObjectName ASC");

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) ROWS FETCH NEXT @PageSize ROWS ONLY;").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<KhVillage, KhCommune, KhDistrict, KhProvince, KhVillage>(sql,
			(obj, commune, district, prv) => {
				district?.Province = prv;
				commune?.District = district;
				obj.Commune = commune;

				return obj;
			}, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public new async Task<List<SearchItemKhVillage>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");

        sbSql.Select("t.Id")
            .Select("t.ObjectCode")
            .Select("t.NameEn")
            .Select("t.NameKh")
            .Select("t.PostalCode")
            .Select("'CommuneId'=c.Id")
            .Select("'CommuneCode'=c.ObjectCode")
            .Select("'CommuneNameEn'=c.NameEn")
            .Select("'CommuneNameKh'=c.NameKh")
            .Select("'DistrictId'=d.Id")
            .Select("'DistrictCode'=d.ObjectCode")
            .Select("'DistrictNameEn'=d.NameEn")
            .Select("'DistrictNameKh'=d.NameKh")
            .Select("'ProvinceId'=p.Id")
            .Select("'ProvinceCode'=p.ObjectCode")
            .Select("'ProvinceNameEn'=p.NameEn")
            .Select("'ProvinceNameKh'=p.NameKh");

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

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{KhCommune.MsSqlTable} c ON c.Id=t.KhCommuneId");
		sbSql.LeftJoin($"{KhDistrict.MsSqlTable} d ON d.Id=c.KhDistrictId");
		sbSql.LeftJoin($"{KhProvince.MsSqlTable} p ON p.Id=d.KhProvinceId");

		sbSql.OrderBy("p.NameEn ASC")
			.OrderBy("d.NameEn ASC")
            .OrderBy("c.NameEn ASC")
			.OrderBy("t.NameEn ASC");


		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) ROWS FETCH NEXT @PageSize ROWS ONLY;").RawSql;
		}

		using var cn = DbContext.DbCxn;
        var dataList = (await cn.QueryAsync<SearchItemKhVillage>(sql, param, commandTimeout: 180)).AsList();
        //var dataList = (await cn.QueryAsync<CambodiaVillage, CambodiaCommune, CambodiaDistrict, CambodiaProvince, CambodiaVillage>(sql,
        //								(obj, c, d, p) =>
        //								{
        //									if (d != null)
        //										d.Province = p;

        //                                          if (c != null)
        //                                              c.District = d;

        //                                          obj.Commune = c;

        //									return obj;
        //								}, param, splitOn: "Id")).AsList();

        return dataList;
	}

	public async Task<List<SearchItemKhVillage>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameKh = null,
        string? nameEn = null,
        string? postalCode = null,
        List<int>? countryCommuneIds = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param= new ();

        sbSql.Select("t.Id")
            .Select("t.ObjectCode")
            .Select("t.NameEn")
            .Select("t.NameKh")
            .Select("t.PostalCode")
            .Select("'CommuneId'=c.Id")
            .Select("'CommuneCode'=c.ObjectCode")
            .Select("'CommuneNameEn'=c.NameEn")
            .Select("'CommuneNameKh'=c.NameKh")
            .Select("'DistrictId'=d.Id")
            .Select("'DistrictCode'=d.ObjectCode")
            .Select("'DistrictNameEn'=d.NameEn")
            .Select("'DistrictNameKh'=d.NameKh")
            .Select("'ProvinceId'=p.Id")
            .Select("'ProvinceCode'=p.ObjectCode")
            .Select("'ProvinceNameEn'=p.NameEn")
            .Select("'ProvinceNameKh'=p.NameKh");

        sbSql.Where("t.IsDeleted=0");

        #region Form Filter Conditions
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

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("t.NameKh LIKE '%'+@NameKh+'%'");
            param.Add("@NameKh", nameKh);
        }

        if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("LOWER(t.NameEn) LIKE '%'+LOWER(@NameEn)+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(postalCode))
        {
            sbSql.Where("t.PostalCode LIKE '%'+@PostalCode+'%'");
            param.Add("@PostalCode", postalCode, DbType.AnsiString);
        }

        if (countryCommuneIds != null)
        {
            if (countryCommuneIds.Count == 1)
            {
                sbSql.Where("t.CambodiaCommuneId=@CambodiaCommuneId");
                param.Add("@CambodiaCommuneId", countryCommuneIds[0]);
            }
            else
            {
                sbSql.Where("t.CambodiaCommuneId IN @CambodiaCommuneIds");
                param.Add("@CambodiaCommuneIds", countryCommuneIds);
            }
        }
		#endregion

		sbSql.LeftJoin($"{KhCommune.MsSqlTable} c ON c.Id=t.KhCommuneId");
		sbSql.LeftJoin($"{KhDistrict.MsSqlTable} d ON d.Id=c.KhDistrictId");
		sbSql.LeftJoin($"{KhProvince.MsSqlTable} p ON p.Id=d.KhProvinceId");

		sbSql.OrderBy("p.NameEn ASC")
			.OrderBy("d.NameEn ASC")
			.OrderBy("c.NameEn ASC")
			.OrderBy("t.NameEn ASC");

		string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) ROWS FETCH NEXT @PageSize ROWS ONLY;").RawSql;
		}

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<SearchItemKhVillage>(sql, param)).AsList();

        //var dataList = (await cn.QueryAsync<CambodiaVillage, CambodiaCommune, CambodiaDistrict, CambodiaProvince, CambodiaVillage>(sql,
        //								(obj, c, d, p) =>
        //								{
        //									if (d != null)
        //										d.Province = p;

        //									if (c != null)
        //										c.District = d;

        //									return obj;
        //								}, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameKh = null,
        string? nameEn = null,
        string? postalCode = null,
        List<int>? countryCommuneIds = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Filter Conditions
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

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("t.NameKh LIKE '%'+@NameKh+'%'");
            param.Add("@NameKh", nameKh);
        }

        if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("LOWER(t.NameEn) LIKE '%'+LOWER(@NameEn)+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(postalCode))
        {
            sbSql.Where("t.PostalCode LIKE '%'+@PostalCode+'%'");
            param.Add("@PostalCode", postalCode, DbType.AnsiString);
        }

        if (countryCommuneIds != null)
        {
            if (countryCommuneIds.Count == 1)
            {
                sbSql.Where("t.KhCommuneId=@KhCommuneId");
                param.Add("@KhCommuneId", countryCommuneIds[0]);
            }
            else
            {
                sbSql.Where("t.KhCommuneId IN @KhCommuneIds");
                param.Add("@KhCommuneIds", countryCommuneIds);
            }
        }
        #endregion

        var sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(KhVillage).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}