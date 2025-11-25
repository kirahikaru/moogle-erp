using DataLayer.Models.Library;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.Library;

public interface IBookGenreRepos : IBaseRepos<BookGenre>
{
	Task<List<BookGenre>> GetByBookAsync(int bookId);

	Task<List<BookGenre>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? parentCode = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? parentCode = null);
}

public class BookGenreRepos(IConnectionFactory connectionFactory) : BaseRepos<BookGenre>(connectionFactory, BookGenre.DatabaseObject), IBookGenreRepos
{
	public async Task<List<BookGenre>> GetByBookAsync(int bookId)
    {
        string sql = $"SELECT bg.* FROM {BookBookGenre.MsSqlTable} bbg LEFT JOIN {BookGenre.MsSqlTable} bg ON bg.Id=bbg.BookGenreId WHERE bbg.BookId=@BookId";

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<BookGenre> result = (await cn.QueryAsync<BookGenre>(sql, new { BookId = bookId })).AsList();

        return result;
    }

    public async Task<List<BookGenre>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? parentCode = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted = 0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(parentCode))
        {
            sbSql.Where("t.ParentCode=@ParentCode");
            param.Add("@ParentCode", parentCode, DbType.AnsiString);
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

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                              $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<BookGenre> result = (await cn.QueryAsync<BookGenre>(sql, param)).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? parentCode = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted = 0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(parentCode))
        {
            sbSql.Where("t.ParentCode=@ParentCode");
            param.Add("@ParentCode", parentCode, DbType.AnsiString);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

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
}