using DataLayer.Models.Library;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.Library;

public interface IBookCategoryRepos : IBaseRepos<BookCategory>
{
	Task<List<BookCategory>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? parentCode = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? parentCode = null);

	Task<List<BasicObjectSelectListItem>> GetValidParentAsync(
		int objectId,
		string? objectCode,
		int? includingId);

	Task<List<DropDownListItem>> GetValidParentsAsync(string? objectCode, string? hierarchyPath, string? searchText = null);
}

public class BookCategoryRepos(IConnectionFactory connectionFactory) : BaseRepos<BookCategory>(connectionFactory, BookCategory.DatabaseObject), IBookCategoryRepos
{
	public async Task<List<BookCategory>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? parentCode = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(parentCode))
        {
            sbSql.Where("ParentCode=@ParentCode");
            param.Add("@ParentCode", parentCode, DbType.AnsiString);
        }
        #endregion

        sbSql.OrderBy("ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT i.* FROM {DbObject.MsSqlTable} i INNER JOIN pg p ON p.Id=i.Id /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<BookCategory> result = (await cn.QueryAsync<BookCategory>(sql, param)).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? parentCode = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(parentCode))
        {
            sbSql.Where("ParentCode=@ParentCode");
            param.Add("@ParentCode", parentCode, DbType.AnsiString);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(BookCategory).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }



    public async Task<List<BasicObjectSelectListItem>> GetValidParentAsync(
        int objectId,
        string? objectCode,
        int? includingId)
    {
        
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("'ObjectId'=t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("t.HierarchyPath");

        param.Add("@Id", objectId);
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);

        if (includingId.HasValue)
        {
            sbSql.Where("(t.IsDeleted=0 AND t.Id<>@Id AND t.HierarchyPath NOT LIKE '%'+@ObjectCode+'%') OR t.Id=@IncludingId");
            param.Add("@IncludingId", includingId.Value);
        }
        else
        {
            sbSql.Where("t.IsDeleted=0");
            sbSql.Where("t.Id<>@Id");
            sbSql.Where("t.HierarchyPath NOT LIKE '%'+@ObjectCode+'%'");
        }

        sbSql.OrderBy("t.ObjectName ASC");

        using var cn = ConnectionFactory.GetDbConnection()!;
        var sql = sbSql.AddTemplate($"SELECT /**select**/  FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

        List<BasicObjectSelectListItem> result = new()
        {
            new BasicObjectSelectListItem()
            {
                ObjectId = 0,
                ObjectCode = "",
                ObjectName = "",
                HierarchyPath = ""
            }
        };

        result.AddRange((await cn.QueryAsync<BasicObjectSelectListItem>(sql, param)).AsList());

        return result;
    }

    public async Task<List<DropDownListItem>> GetValidParentsAsync(
        string? objectCode,
        string? hierarchyPath,
        string? searchText = null)
    {
        if (string.IsNullOrEmpty(objectCode) && string.IsNullOrEmpty(hierarchyPath))
            return new List<DropDownListItem>();

        SqlBuilder sbSql = new();

        sbSql.Select("t.Id")
            .Select("'ObjectType'='ItemCategory'")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("t.HierarchyPath");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.ObjectCode<>@ObjectCode");
        sbSql.Where("t.HierarchyPath NOT LIKE @HierarchyPath+'%'");

        DynamicParameters param = new();

        param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        sbSql.OrderBy("t.ObjectName ASC");

        using var cn = ConnectionFactory.GetDbConnection()!;
        var sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        List<DropDownListItem> dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).ToList();
        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        int? parentId = null,
        string? parentCode = null,
        string? objectCode = null,
        string? objectName = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        //Implement lower-case convert for text-type base search to lower is to ensure searching is case-insensitive
        //since now PCLAAPP DB is a unicode enabled database, i.e. all search are case sensitive

        sbSql.Where("t.IsDeleted=0");

        if (parentId.HasValue)
        {
            sbSql.Where("t.ParentId=@ParentId");
            param.Add("@ParentId", parentId);
        }

        if (!string.IsNullOrEmpty(parentCode))
        {
            sbSql.Where("LOWER(t.ParentCode) LIKE '%'+LOWER(@ParentCode)+'%'");
            param.Add("@ParentCode", parentCode, DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+LOWER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(BookCategory).Name,
            RecordCount = (int)recordCount,
            PageCount = pageCount,
            PageSize = pgSize
        };

        return pagination;
    }
}