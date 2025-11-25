using DataLayer.Models.SystemCore.NonPersistent;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.SystemCore;

public interface IOccupationRepos : IBaseRepos<Occupation>
{
	Task<Occupation?> GetFullAsync(int id);

	Task<List<Occupation>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		List<int>? occupationCategoryIdList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		List<int>? occupationCategoryIdList = null);
}

public class OccupationRepos(IConnectionFactory connectionFactory) : BaseRepos<Occupation>(connectionFactory, Occupation.DatabaseObject), IOccupationRepos
{
	public async Task<Occupation?> GetFullAsync(int id)
    {
		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");
        param.Add("@Id", id);

        sbSql.LeftJoin($"{OccupationCategory.MsSqlTable} oc ON oc.Id=t.OccupationCategoryId");

		using var cn = ConnectionFactory.GetDbConnection()!;
        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        var data = (await cn.QueryAsync<Occupation, OccupationCategory, Occupation>(sql,
                                (obj, category) =>
                                {
                                    obj.Category = category;
                                    return obj;
                                }, param, splitOn: "Id")).FirstOrDefault();

        return data;
	}

	public override async Task<List<Occupation>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
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

        sbSql.LeftJoin($"{OccupationCategory.MsSqlTable} oc ON oc.Id=t.OccupationCategoryId");
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
				$"SELECT t.*, oc.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = (await cn.QueryAsync<Occupation, OccupationCategory, Occupation>(sql, (obj, category) =>
		{
			obj.Category = category;
			return obj;

		}, param, splitOn: "Id")).AsList();

		return dataList;
	}

	public async Task<List<Occupation>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null,
        List<int>? occupationCategoryIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgNo), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("LOWER(t.[Title]) LIKE '%'+@Title+'%'");
            param.Add("@Title", objectNameKh);
        }

        if (occupationCategoryIdList != null && occupationCategoryIdList.Any())
        {
            if (occupationCategoryIdList.Count == 1)
            {
                sbSql.Where("t.OccupationCategoryId IS NOT NULL");
                sbSql.Where("t.OccupationCategoryId=@OccupationCategoryId");
                param.Add("@OccupationCategoryId", occupationCategoryIdList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.OccupationCategoryId IN @OccupationCategoryIdList");
                param.Add("@OccupationCategoryIdList", occupationCategoryIdList);
            }
        }
        #endregion

        sbSql.LeftJoin($"{OccupationCategory.MsSqlTable} oc ON oc.Id=t.OccupationCategoryId");

        sbSql.OrderBy("t.ObjectName ASC");

        using var cn = ConnectionFactory.GetDbConnection()!;
        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                    $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        var dataList = (await cn.QueryAsync<Occupation, OccupationCategory, Occupation>(sql, (obj, category) =>
                                    {
                                        obj.Category = category;
                                        return obj;

                                    },param, splitOn:"Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null,
        List<int>? occupationCategoryIdList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("LOWER(t.[Title]) LIKE '%'+@Title+'%'");
            param.Add("@Title", objectNameKh);
        }

        if (occupationCategoryIdList != null && occupationCategoryIdList.Any())
        {
            if (occupationCategoryIdList.Count == 1)
            {
                sbSql.Where("t.OccupationCategoryId IS NOT NULL");
                sbSql.Where("t.OccupationCategoryId=@OccupationCategoryId");
                param.Add("@OccupationCategoryId", occupationCategoryIdList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.OccupationCategoryId IN @OccupationCategoryIdList");
                param.Add("@OccupationCategoryIdList", occupationCategoryIdList);
            }
        }
        #endregion

        using var cn = ConnectionFactory.GetDbConnection()!;

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Occupation).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}