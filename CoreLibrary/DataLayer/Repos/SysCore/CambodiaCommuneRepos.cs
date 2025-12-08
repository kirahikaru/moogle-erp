using DataLayer.Models.SysCore.NonPersistent;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.SysCore;

public interface ICambodiaCommuneRepos : IBaseRepos<CambodiaCommune>
{
	Task<CambodiaCommune?> GetFullAsync(int id);

	Task<CambodiaCommune?> GetGivenVillageAsync(int cambodiaVillageId);

	Task<List<DropDownListItem>> GetForDropdownSelect1Async(int? cambodiaDistrictId, string? searchText = null);
	Task<List<DropDownListItem>> GetForDropdownSelectFullTextAsync(int? cambodiaDistrictId, string? searchText = null);

	Task<List<CambodiaCommune>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameKh = null,
		string? nameEn = null,
		string? postalCode = null,
		List<int>? countryDistrictIds = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameKh = null,
		string? nameEn = null,
		string? postalCode = null,
		List<int>? countryDistrictIds = null);
}

public class CambodiaCommuneRepos(IDbContext dbContext) : BaseRepos<CambodiaCommune>(dbContext, CambodiaCommune.DatabaseObject), ICambodiaCommuneRepos
{
	public async Task<CambodiaCommune?> GetFullAsync(int id)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");

        param.Add("@Id", id);

        sbSql.LeftJoin($"{CambodiaDistrict.MsSqlTable} d ON d.Id=t.CambodiaDistrictId");
		sbSql.LeftJoin($"{CambodiaProvince.MsSqlTable} p ON p.Id=d.CambodiaProvinceId");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        var dataList = (await cn.QueryAsync<CambodiaCommune, CambodiaDistrict, CambodiaProvince, CambodiaCommune>(sql,
                                (obj, d, p) =>
                                {
                                    if (d != null)
                                        d.Province = p;

                                    obj.District = d;

                                    return obj;
                                }, param, splitOn: "Id")).AsList();

        if (dataList != null && dataList.Count != 0)
            return dataList[0];
        else
            return null;
	}

	public async Task<CambodiaCommune?> GetGivenVillageAsync(int cambodiaVillageId)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("v.Id=@Id");
        sbSql.LeftJoin($"{CambodiaVillage.MsSqlTable} v ON v.IsDeleted=0 AND v.CambodiaCommuneId=t.Id");

        param.Add("@Id", cambodiaVillageId);

        var sql = sbSql.AddTemplate($"SELECT c.* FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        return await cn.QuerySingleOrDefaultAsync<CambodiaCommune?>(sql, param);
    }

    public async Task<List<DropDownListItem>> GetForDropdownSelect1Async(int? cambodiaDistrictId, string? searchText = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Select("'ObjectId'=t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("'ObjectType'=(CASE t.[Type] WHEN 'S' THEN 'SANGKAT' WHEN 'C' THEN 'COMMUNE' ELSE t.[Type] END)")
            .Select("'ObjectNameEn'=t.NameEn")
            .Select("'ObjectNameKh'=t.NameKh");

        sbSql.Where("t.IsDeleted=0");

        if (cambodiaDistrictId.HasValue)
        {
            sbSql.Where("t.CambodiaDistrictId IS NOT NULL");
            sbSql.Where("t.CambodiaDistrictId=@CambodiaDistrictId");
            param.Add("@CambodiaDistrictId", cambodiaDistrictId.Value);
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

        var sbSqlTempl = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/");

        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<DropDownListItem>(sbSqlTempl.RawSql, param)).AsList();
    }

    public async Task<List<DropDownListItem>> GetForDropdownSelectFullTextAsync(int? cambodiaDistrictId, string? searchText = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("'ObjectId'=cc.Id")
            .Select("cc.ObjectCode")
            .Select("'ObjectName'=cc.NameEn+', '+ISNULL(cd.NameEn,'-')+', '+ISNULL(cp.NameEn,'-')+ ' ('+cc.PostalCode+')'")
            .Select("'ObjectType'=(CASE cc.[Type] WHEN 'S' THEN 'SANGKAT' WHEN 'C' THEN 'COMMUNE' ELSE cc.[Type] END)")
            .Select("'ObjectNameEn'=cc.NameEn")
            .Select("'ObjectNameKh'=cc.NameKh");

        sbSql.Where("cc.IsDeleted=0");

        if (cambodiaDistrictId.HasValue)
        {
            sbSql.Where("cc.CambodiaDistrictId IS NOT NULL");
            sbSql.Where("cc.CambodiaDistrictId=@CambodiaDistrictId");
            param.Add("@CambodiaDistrictId", cambodiaDistrictId.Value);
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("UPPER(cc.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else
            {
                sbSql.Where("(UPPER(cc.NameEn) LIKE '%'+UPPER(@SearchText)+'%' OR t.NameKh LIKE '%'+@SearchText+'%')");
            }

            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        sbSql.LeftJoin($"{CambodiaDistrict.MsSqlTable} cd ON cd.IsDeleted=0 AND cd.Id=cc.CambodiaDistrictId");
        sbSql.LeftJoin($"{CambodiaProvince.MsSqlTable} cp ON cp.IsDeleted=0 AND cp.Id=cd.CambodiaProvinceId");

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} cc /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();
    }

	public override async Task<KeyValuePair<int, IEnumerable<CambodiaCommune>>> SearchNewAsync(
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

		sbSql.LeftJoin($"{CambodiaDistrict.MsSqlTable} d ON d.Id=t.KhDistrictId");
		sbSql.LeftJoin($"{CambodiaProvince.MsSqlTable} prv ON prv.Id=d.KhProvinceId");

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
			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<CambodiaCommune, CambodiaDistrict, CambodiaProvince, CambodiaCommune>(sql,
			(obj, district, prv) => {

                district?.Province = prv;
				obj.District = district;

				return obj;
			}, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public override async Task<List<CambodiaCommune>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null,
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

        sbSql.LeftJoin($"{CambodiaDistrict.MsSqlTable} d ON d.Id=t.CambodiaDistrictId");
		sbSql.LeftJoin($"{CambodiaProvince.MsSqlTable} prv ON prv.Id=d.CambodiaProvinceId");

		sbSql.OrderBy("t.NameEn ASC")
			.OrderBy("t.NameKh ASC");

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
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT t.*, d.*, prv.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;
		var dataList = (await cn.QueryAsync<CambodiaCommune, CambodiaDistrict, CambodiaProvince, CambodiaCommune>(sql,
										(obj, d, p) =>
										{
											if (d != null)
												d.Province = p;

											obj.District = d;

											return obj;
										}, param, splitOn: "Id")).AsList();
		return dataList;
	}

	public async Task<List<CambodiaCommune>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameKh = null,
        string? nameEn = null,
        string? postalCode = null,
        List<int>? countryDistrictIds = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region FILTER CONDITIONS
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE '%'+@ObjectCode+'%'");
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
            sbSql.Where("LOWER(t.NameEn) LIKE '%'+@NameEn+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(postalCode))
        {
            sbSql.Where("t.PostalCode LIKE '%'+@PostalCode+'%'");
            param.Add("@PostalCode", postalCode, DbType.AnsiString);
        }

        if (countryDistrictIds != null)
        {
            if (countryDistrictIds.Count == 1)
            {
                sbSql.Where("t.CambodiaDistrictId=@CambodiaDistrictId");
                param.Add("@CambodiaDistrictId", countryDistrictIds[0]);
            }
            else
            {
                sbSql.Where("t.CambodiaDistrictId IN @CambodiaDistrictIds");
                param.Add("@CambodiaDistrictIds", countryDistrictIds);
            }
        }
		#endregion

		sbSql.LeftJoin($"{CambodiaDistrict.MsSqlTable} d ON d.Id=t.CambodiaDistrictId");
		sbSql.LeftJoin($"{CambodiaProvince.MsSqlTable} prv ON prv.Id=d.CambodiaProvinceId");

		sbSql.OrderBy("t.NameEn ASC")
			.OrderBy("t.NameKh ASC");

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
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.*, d.*, prv.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<CambodiaCommune, CambodiaDistrict, CambodiaProvince, CambodiaCommune>(sql,
                                        (obj, d, p) =>
                                        {
                                            if (d != null)
                                                d.Province = p;

                                            obj.District = d;

                                            return obj;
                                        }, param, splitOn:"Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameKh = null,
        string? nameEn = null,
        string? postalCode = null,
        List<int>? countryDistrictIds = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region FILTER CONDITIONS
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE '%'+@ObjectCode+'%'");
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
            sbSql.Where("LOWER(t.NameEn) LIKE '%'+@NameEn+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(postalCode))
        {
            sbSql.Where("t.PostalCode LIKE '%'+@PostalCode+'%'");
            param.Add("@PostalCode", postalCode, DbType.AnsiString);
        }

        if (countryDistrictIds != null)
        {
            if (countryDistrictIds.Count == 1)
            {
                sbSql.Where("t.CambodiaDistrictId=@CambodiaDistrictId");
                param.Add("@CambodiaDistrictId", countryDistrictIds[0]);
            }
            else
            {
                sbSql.Where("t.CambodiaDistrictId IN @CambodiaDistrictIds");
                param.Add("@CambodiaDistrictIds", countryDistrictIds);
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(CambodiaCommune).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}