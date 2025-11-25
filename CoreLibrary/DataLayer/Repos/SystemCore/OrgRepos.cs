using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface IOrgRepos : IBaseRepos<Organization>
{
	Task<List<Organization>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? industryCode = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? industryCode = null);
}

public class OrgRepos(IConnectionFactory connectionFactory) : BaseRepos<Organization>(connectionFactory, Organization.DatabaseObject), IOrgRepos
{
	public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? industryCode = null)
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
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(industryCode))
        {
            sbSql.Where("LOWER(t.IndustryCode)=LOWER(@IndustryCode)");
            param.Add("@IndustryCode", industryCode, DbType.AnsiString);
        }
        #endregion

        using var cn = ConnectionFactory.GetDbConnection()!;
        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Organization).Name,
            PageSize = pgSize,
            RecordCount = (int)recordCount,
            PageCount = pageCount
        };

        return pagination;
    }


    public async Task<List<Organization>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? industryCode = null)
    {
        if (pgNo < 0 && pgSize < 0)
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
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(industryCode))
        {
            sbSql.Where("LOWER(t.IndustryCode)=LOWER(@IndustryCode)");
            param.Add("@IndustryCode", industryCode, DbType.AnsiString);
        }
        #endregion

        sbSql.OrderBy("t..ObjectName ASC");

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

        using var cn = ConnectionFactory.GetDbConnection()!;

        return (await cn.QueryAsync<Organization>(sql, param)).ToList();
    }
}