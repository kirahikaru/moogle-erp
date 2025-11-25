using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Repos.SysCore;

public interface IProductCategoryRepos : IBaseRepos<ProductCategory>
{
	Task<List<ProductCategory>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		int? organizationId = null,
		int? branchId = null,
		string? objectName = null,
		bool? isEnabled = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		int? organizationId = null,
		int? branchId = null,
		string? objectName = null,
		bool? isEnabled = null);
}

public class ProductCategoryRepos(IDbContext dbContext) : BaseRepos<ProductCategory>(dbContext, ProductCategory.DatabaseObject), IProductCategoryRepos
{
	public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        int? organizationId = null,
        int? branchId = null,
        string? objectName = null,
        bool? isEnabled = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        //Implement lower-case convert for text-type base search to lower is to ensure searching is case-insensitive
        //since now PCLAAPP DB is a unicode enabled database, i.e. all search are case sensitive

        if (organizationId.HasValue)
        {
            sbSql.Where("t..OrganizationId=@OrganizationId");
            param.Add("@OrganizationId", organizationId.Value);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName);
        }

        if (isEnabled.HasValue)
        {
            sbSql.Where("t.IsEnabled=@IsEnabled");
            param.Add("@IsEnabled", isEnabled.Value);
        }

        var sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(User).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<ProductCategory>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        int? organizationId = null,
        int? branchId = null,
        string? objectName = null,
        bool? isEnabled = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        //Implement lower-case convert for text-type base search to lower is to ensure searching is case-insensitive
        //since now PCLAAPP DB is a unicode enabled database, i.e. all search are case sensitive

        if (organizationId > 0)
        {
            sbSql.Where("t.OrganizationId=@OrganizationId");
            param.Add("@OrganizationId", organizationId);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(CultureInfo.CurrentUICulture));
        }

        if (isEnabled.HasValue)
        {
            sbSql.Where("t.IsEnabled=@IsEnabled");
            param.Add("@IsEnabled", isEnabled.Value);
        }

        sbSql.OrderBy("t.ObjectName ASC");

        string? sql;
        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            //throw new NotImplementedException();
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate($";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=c.Id /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<ProductCategory>(sql, param)).ToList();
    }
}