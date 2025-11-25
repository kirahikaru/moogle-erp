using DataLayer.Models.SystemCore.NonPersistent;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.SystemCore;

public interface ICambodiaDistrictRepos : IBaseRepos<CambodiaDistrict>
{
	Task<CambodiaDistrict?> GetFullAsync(int id);

	Task<List<DropDownListItem>> GetForDropdownSelect1Async(int? cambodiaProvinceId, string? searchText = null);
	Task<List<DropDownListItem>> GetForDropdownSelectFullTextAsync(int? cambodiaProvinceId, string? searchText = null);

	/// <summary>
	/// Get Province given a known sub address e.g. known Commune or know Village
	/// </summary>
	/// <param name="subAddrObjectTypeName">
	/// If know commune then 'CambodiaCommune'
	/// If know village then 'CambodiaVilalge'
	/// </param>
	/// <param name="subAddrId"></param>
	/// <returns></returns>
	Task<CambodiaDistrict?> GetGivenSubAddressAsync(string subAddrObjectTypeName, int subAddrId);

	Task<List<CambodiaDistrict>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameKh = null,
		string? nameEn = null,
		string? postalCode = null,
		List<int>? countryProvinceIds = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameKh = null,
		string? nameEn = null,
		string? postalCode = null,
		List<int>? countryProvinceIds = null);
}

public class CambodiaDistrictRepos(IConnectionFactory connectionFactory) : BaseRepos<CambodiaDistrict>(connectionFactory, CambodiaDistrict.DatabaseObject), ICambodiaDistrictRepos
{
	public async Task<CambodiaDistrict?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        param.Add("@Id", id);

        sbSql.LeftJoin($"{CambodiaProvince.MsSqlTable} p ON p.Id=t.CambodiaProvinceId");

        using var cn = ConnectionFactory.GetDbConnection()!;
        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        var dataList = (await cn.QueryAsync<CambodiaDistrict, CambodiaProvince, CambodiaDistrict>(sql,
                                (obj, p) =>
                                {
                                    obj.Province = p;

                                    return obj;
                                }, param, splitOn: "Id")).AsList();

        if (dataList != null && dataList.Count != 0)
            return dataList[0];
        else
            return null;
    }

    public async Task<List<DropDownListItem>> GetForDropdownSelect1Async(int? cambodiaProvinceId, string? searchText = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("'ObjectId'=t.Id");
        sbSql.Select("t.ObjectCode");
        sbSql.Select("t.ObjectName");
        sbSql.Select("'ObjectType'=CASE t.[Type] WHEN 'C' THEN 'CITY' WHEN 'K' THEN 'KHAN' WHEN 'S' THEN 'SROK' ELSE t.[Type] END");
        sbSql.Select("'ObjectNameEn'=t.NameEn");
        sbSql.Select("'ObjectNameKh'=t.NameKh");

        sbSql.Where("t.IsDeleted=0");

        if (cambodiaProvinceId.HasValue)
        {
            sbSql.Where("t.CambodiaProvinceId IS NOT NULL");
            sbSql.Where("t.CambodiaProvinceId=@CambodiaProvinceId");

            param.Add("@CambodiaProvinceId", cambodiaProvinceId.Value);
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
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }
        sbSql.OrderBy("t.ObjectName ASC");

        using var cn = ConnectionFactory.GetDbConnection()!;

        var sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

        return (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();
    }

    public async Task<List<DropDownListItem>> GetForDropdownSelectFullTextAsync(int? cambodiaProvinceId, string? searchText = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("'ObjectId'=d.Id")
            .Select("d.ObjectCode")
            .Select("'ObjectName'=ISNULL(d.NameEn,'-')+', '+ISNULL(p.NameEn,'-')+ ' ('+d.PostalCode+')'")
            .Select("'ObjectType'=(CASE d.[Type] WHEN 'C' THEN 'City' WHEN 'K' THEN 'Khan' WHEN 'S' THEN 'District' ELSE d.[Type] END)")
            .Select("'ObjectNameEn'=d.NameEn")
            .Select("'ObjectNameKh'=d.NameKh");

        sbSql.Where("d.IsDeleted=0");

        if (cambodiaProvinceId.HasValue)
        {
            sbSql.Where("d.CambodiaProvinceId IS NOT NULL");
            sbSql.Where("d.CambodiaProvinceId=@CambodiaProvinceId");
            param.Add("@CambodiaProvinceId", cambodiaProvinceId.Value);
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("UPPER(d.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else
            {
                sbSql.Where("(UPPER(d.NameEn) LIKE '%'+UPPER(@SearchText)+'%' OR t.NameKh LIKE '%'+@SearchText+'%')");
            }

            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        sbSql.LeftJoin($"{CambodiaProvince.MsSqlTable} p ON p.IsDeleted=0 AND p.Id=d.CambodiaProvinceId");

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} d /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        return (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();
    }

    public async Task<CambodiaDistrict?> GetGivenSubAddressAsync(string subAddrObjectTypeName, int subAddrId)
    {
        SqlBuilder sbSql = new();
        

        switch (subAddrObjectTypeName)
        {
            case nameof(CambodiaCommune):
                {
                    sbSql.LeftJoin($"{CambodiaCommune.MsSqlTable} c ON c.IsDeleted=0 AND c.CambodiaDistrictId=d.Id");
                    sbSql.Where("d.IsDeleted=0");
                    sbSql.Where("c.Id=@Id");
                }
                break;
            case nameof(CambodiaVillage):
                {
                    sbSql.LeftJoin($"{CambodiaCommune.MsSqlTable} c ON c.IsDeleted=0 AND c.CambodiaDistrictId=d.Id");
                    sbSql.LeftJoin($"{CambodiaVillage.MsSqlTable} v ON v.IsDeleted=0 AND v.CambodiaCommuneId=c.Id)");
                    sbSql.Where("d.IsDeleted=0");
                    sbSql.Where("v.Id=@Id");
                }
                break;
            default:
                return null;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        string sql = sbSql.AddTemplate($"SELECT d.* FROM {CambodiaDistrict.MsSqlTable} d /**leftjoin**/ /**where**/").RawSql;
        return await cn.QuerySingleOrDefaultAsync<CambodiaDistrict>(sql, new { Id = subAddrId });
    }

	public override async Task<List<CambodiaDistrict>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
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

        sbSql.LeftJoin($"{CambodiaProvince.MsSqlTable} prv ON prv.Id=t.CambodiaProvinceId");

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
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT t.*, prv.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var data = (await cn.QueryAsync<CambodiaDistrict, CambodiaProvince, CambodiaDistrict>(sql,
								(obj, prv) =>
								{
									obj.Province = prv;
									return obj;
								}, param, splitOn: "Id")).AsList();

		return data;
	}

	public async Task<List<CambodiaDistrict>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameKh = null,
        string? nameEn = null,
        string? postalCode = null,
        List<int>? countryProvinceIds = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Condition
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

        if (countryProvinceIds != null)
        {
            if (countryProvinceIds.Count == 1)
            {
                sbSql.Where("t.CambodiaProvinceId=@CambodiaProvinceId");
                param.Add("@CambodiaProvinceId", countryProvinceIds[0]);
            }
            else
            {
                sbSql.Where("t.CambodiaProvinceId IN @CambodiaProvinceIds");
                param.Add("@CambodiaProvinceIds", countryProvinceIds);
            }
        }
        #endregion

        sbSql.LeftJoin($"{CambodiaProvince.MsSqlTable} prv ON prv.Id=t.CambodiaProvinceId");

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

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.*, prv.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=a.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<CambodiaDistrict, CambodiaProvince, CambodiaDistrict>(sql,
                                (obj, prv) =>
                                {
                                    obj.Province = prv;
                                    return obj;
                                }, param, splitOn:"Id")).AsList();

        return data;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameKh = null,
        string? nameEn = null,
        string? postalCode = null,
        List<int>? countryProvinceIds = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Condition
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

        if (countryProvinceIds != null)
        {
            if (countryProvinceIds.Count == 1)
            {
                sbSql.Where("t.CambodiaProvinceId=@CambodiaProvinceId");
                param.Add("@CambodiaProvinceId", countryProvinceIds[0]);
            }
            else
            {
                sbSql.Where("t.CambodiaProvinceId IN @CambodiaProvinceIds");
                param.Add("@CambodiaProvinceIds", countryProvinceIds);
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(CambodiaDistrict).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}