using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface ICambodiaProvinceRepos : IBaseRepos<CambodiaProvince>
{
	Task<List<DropDownListItem>> GetForDropdownSelect1Aysnc(string? searchText = null);

	/// <summary>
	/// Get Province given a known sub address e.g. known District, known Commune or know Village
	/// </summary>
	/// <param name="subAddrObjectTypeName">
	/// If know district then 'CambodiaDistrict'
	/// If know commune then 'CambodiaCommune'
	/// If know village then 'CambodiaVilalge'
	/// </param>
	/// <param name="subAddrId"></param>
	/// <returns></returns>
	Task<CambodiaProvince?> GetGivenSubAddressAsync(string subAddrObjectTypeName, int subAddrId);

	Task<List<CambodiaProvince>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameKh = null,
		string? nameEn = null,
		string? code2 = null,
		string? code3 = null,
		string? postalCode = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameKh = null,
		string? nameEn = null,
		string? code2 = null,
		string? code3 = null,
		string? postalCode = null);
}


public class CambodiaProvinceRepos(IConnectionFactory connectionFactory) : BaseRepos<CambodiaProvince>(connectionFactory, CambodiaProvince.DatabaseObject), ICambodiaProvinceRepos
{
	public async Task<List<DropDownListItem>> GetForDropdownSelect1Aysnc(string? searchText = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");

        sbSql.Select("'ObjectId'=t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("'ObjectType'='CambodiaProvince'")
            .Select("'ObjectNameEn'=t.NameEn")
            .Select("'ObjectNameKh'=t.NameKh");

        if(!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else
            {
                sbSql.Where("(UPPER(t.NameEn) LIKE '%'+UPPER(@SearchText)+'%' OR t.NameKh LIKE '%'+@SearchText+'%')");
                param.Add("@SearchText", searchText);
            }
        }

        sbSql.OrderBy("t.ObjectName ASC");

        using var cn = ConnectionFactory.GetDbConnection()!;

        var sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

        return (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();
    }

    public async Task<CambodiaProvince?> GetGivenSubAddressAsync(string subAddrObjectTypeName, int subAddrId)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");

        switch (subAddrObjectTypeName)
        {
            case nameof(CambodiaDistrict):
                {
                    sbSql.LeftJoin($"{CambodiaDistrict.MsSqlTable} d ON d.IsDeleted=0 AND d.CambodiaProvinceId=t.Id");
                    sbSql.Where("d.Id = @Id");
                }
                
                break;
            case nameof(CambodiaCommune):
                {
                    sbSql.LeftJoin($"{CambodiaDistrict.MsSqlTable} d ON d.IsDeleted=0 AND d.CambodiaProvinceId=t.Id");
                    sbSql.LeftJoin($"{CambodiaCommune.MsSqlTable} c ON c.IsDeleted=0 AND c.CambodiaDistrictId=d.Id");
                    sbSql.Where("c.Id=@Id");
                }
                break;
            case nameof(CambodiaVillage):
                {
                    sbSql.LeftJoin($"{CambodiaDistrict.MsSqlTable} d ON d.IsDeleted=0 AND d.CambodiaProvinceId=t.Id");
                    sbSql.LeftJoin($"{CambodiaCommune.MsSqlTable} c ON c.IsDeleted=0 AND c.CambodiaDistrictId=d.Id");
                    sbSql.LeftJoin($"{CambodiaVillage.MsSqlTable} v ON v.IsDeleted=0 AND v.CambodiaCommuneId=c.Id");
                    sbSql.Where("v.Id=@Id");
                }
                break;
            default:
                return null;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        string sql = sbSql.AddTemplate($"SELECT t.* FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        return await cn.QuerySingleOrDefaultAsync<CambodiaProvince>(sql, new { Id = subAddrId });
    }

    public async Task<List<CambodiaProvince>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameKh = null,
        string? nameEn = null,
        string? code2 = null,
        string? code3 = null,
        string? postalCode = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("t.NameKh LIKE '%'+@NameKh+'%'");
            param.Add("@NameKh", nameKh);
        }

        if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("LOWER(t.NameEn) LIKE '%'+@NameEn+'%'");
            param.Add("@NameEn", nameEn.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(code2))
        {
            sbSql.Where("UPPER(t.Code2)=@Code2");
            param.Add("@Code2", code2.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(code3))
        {
            sbSql.Where("UPPER(t.Code3)=@Code3");
            param.Add("@Code3", code3.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(postalCode))
        {
            sbSql.Where("t.PostalCode LIKE '%'+@PostalCode+'%'");
            param.Add("@PostalCode", postalCode, DbType.AnsiString);
        }
        #endregion

        sbSql.OrderBy("t.ObjectName ASC");

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
                    $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                    $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<CambodiaProvince> data = (await cn.QueryAsync<CambodiaProvince>(sql, param)).AsList();

        return data;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameKh = null,
        string? nameEn = null,
        string? code2 = null,
        string? code3 = null,
        string? postalCode = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("t.NameKh LIKE '%'+@NameKh+'%'");
            param.Add("@NameKh", nameKh);
        }

        if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("LOWER(t.NameEn) LIKE '%'+@NameEn+'%'");
            param.Add("@NameEn", nameEn.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(code2))
        {
            sbSql.Where("UPPER(t.Code2)=@Code2");
            param.Add("@Code2", code2.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(code3))
        {
            sbSql.Where("UPPER(t.Code3)=@Code3");
            param.Add("@Code3", code3.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(postalCode))
        {
            sbSql.Where("t.PostalCode LIKE '%'+@PostalCode+'%'");
            param.Add("@PostalCode", postalCode, DbType.AnsiString);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(CambodiaProvince).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}