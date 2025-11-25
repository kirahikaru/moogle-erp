using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface IUoMRepos : IBaseRepos<UnitOfMeasure>
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="unitOfMeasureType"></param>
	/// <param name="dispOpt">Dropdown Text Display Option</param>
	/// <returns></returns>
	Task<List<DropdownSelectItem>> GetForDropdownAsync(string? unitOfMeasureType = null, UomTextDisplayOption dispOpt = UomTextDisplayOption.NameOnly);

	Task<List<UnitOfMeasure>> GetByTypeAsync(string unitOfMeasureType);

	Task<List<UnitOfMeasure>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		List<string>? typeList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		List<string>? typeList = null);
}

public enum UomTextDisplayOption
{
	NameOnly, NameAndSymbol, SymbolOnly
}

public class UoMRepos(IConnectionFactory connectionFactory) : BaseRepos<UnitOfMeasure>(connectionFactory, UnitOfMeasure.DatabaseObject), IUoMRepos
{
	public async Task<List<UnitOfMeasure>> GetByTypeAsync(string unitOfMeasureType)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UnitOfMeasureType=@UnitOfMeasureType");

        sbSql.OrderBy("t.ObjectName");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        var list = (await cn.QueryAsync<UnitOfMeasure>(sql, new { UnitOfMeasureType = new DbString { Value = unitOfMeasureType, IsAnsi = true } })).AsList();
        return list;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdownAsync(string? unitOfMeasureType = null, UomTextDisplayOption dispOpt = UomTextDisplayOption.NameOnly)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        if (!string.IsNullOrEmpty(unitOfMeasureType))
        {
            param.Add("@UnitOfMeasureType", unitOfMeasureType, DbType.AnsiString);
            sbSql.Where("t.UnitOfMeasureType=@UnitOfMeasureType");
        }

        sbSql.Select("t.Id")
            .Select("'Key'=t.ObjectCode");

        switch (dispOpt)
        {
            case UomTextDisplayOption.NameOnly:
                sbSql.Select("'Value'=t.ObjectName");
                break;

            case UomTextDisplayOption.NameAndSymbol:
                sbSql.Select("'Value'=t.ObjectName+' ('+t.UnitSymbol+')'");
                break;

            case UomTextDisplayOption.SymbolOnly:
                sbSql.Select("'Value'=t.UnitSymbol");
                break;
        }

        sbSql.Where("t.IsDeleted=0");

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        var list = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();
        return list;
    }

    public async Task<List<UnitOfMeasure>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        List<string>? typeList = null)
    {
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
            param.Add("@ObjectName", objectName!, DbType.AnsiString);
        }

        if (typeList != null && typeList.Any())
        {
            if (typeList.Count == 1)
            {
                sbSql.Where("t.UnitOfMeasureType=@UnitOfMeasureType");
                param.Add("@UnitOfMeasureType", typeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.UnitOfMeasureType IN @UnitOfMeasureTypeList");
                param.Add("@UnitOfMeasureTypeList", typeList);
            }
        }
        #endregion

        sbSql.OrderBy("t.UnitOfMeasureType ASC, t.ObjectName ASC");

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
                    $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON t.Id=p.Id /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<UnitOfMeasure> dataList = (await cn.QueryAsync<UnitOfMeasure>(sql, param)).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        List<string>? typeList = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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
            param.Add("@ObjectName", objectName!, DbType.AnsiString);
        }

        if (typeList != null && typeList.Any())
        {
            if (typeList.Count == 1)
            {
                sbSql.Where("t.UnitOfMeasureType=@UnitOfMeasureType");
                param.Add("@UnitOfMeasureType", typeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.UnitOfMeasureType IN @UnitOfMeasureTypeList");
                param.Add("@UnitOfMeasureTypeList", typeList);
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)(Math.Ceiling(recordCount / pgSize));

        DataPagination pagingResult = new()
        {
            ObjectType = typeof(Country).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagingResult;
    }
}