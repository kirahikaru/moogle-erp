using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface IOccupationCategoryRepos : IBaseRepos<OccupationCategory>
{
	Task<List<OccupationCategory>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null);
}

public class OccupationCategoryRepos(IConnectionFactory connectionFactory) : BaseRepos<OccupationCategory>(connectionFactory, OccupationCategory.DatabaseObject), IOccupationCategoryRepos
{
	public async Task<List<OccupationCategory>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgNo), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("t.ObjectName LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("LOWER(t.[Title]) LIKE '%'+@Title+'%'");
            param.Add("@Title", objectNameKh);
        }
        #endregion

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
                                    $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        List<OccupationCategory> result = (await cn.QueryAsync<OccupationCategory>(sql, param)).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("t.ObjectName LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("LOWER(t.[Title]) LIKE '%'+@Title+'%'");
            param.Add("@Title", objectNameKh);
        }
        #endregion

        using var cn = ConnectionFactory.GetDbConnection()!;

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(OccupationCategory).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}