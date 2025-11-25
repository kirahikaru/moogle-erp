using DataLayer.Models.Hobby;

namespace DataLayer.Repos.Hobby;

public interface IBoardgameContentItemRepos : IBaseRepos<BoardgameContentItem>
{
	Task<List<BoardgameContentItem>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectName = null,
		string? objectCode = null,
		string? boardGameName = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? boardGameName = null);

	Task<List<BoardgameContentItem>> GetByBoardgameAsync(int boardgameId);
}

public class BoardgameContentItemRepos(IDbContext dbContext) : BaseRepos<BoardgameContentItem>(dbContext, BoardgameContentItem.DatabaseObject), IBoardgameContentItemRepos
{
	public async Task<List<BoardgameContentItem>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? boardGameName = null)
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

        if (!string.IsNullOrEmpty(boardGameName))
        {
            sbSql.Where("LOWER(bg.ObjectName) LIKE '%'+LOWER(@BoardgameName)+'%'");
            param.Add("@BoardgameName", boardGameName, DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{Boardgame.MsSqlTable} bg ON bg.Id=t.BoardgameId");

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

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                    $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var result = (await cn.QueryAsync<BoardgameContentItem>(sql, param)).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? boardGameName = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), "Page Size cannot be negative value.");

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

        if (!string.IsNullOrEmpty(boardGameName))
        {
            sbSql.Where("LOWER(bg.ObjectName) LIKE '%'+LOWER(@BoardgameName)+'%'");
            param.Add("@BoardgameName", boardGameName, DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{Boardgame.MsSqlTable} bg ON bg.Id=t.BoargameId");

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**lefjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Boardgame).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<BoardgameContentItem>> GetByBoardgameAsync(int boardgameId)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND BoardgameId=@BoardgameId";
        var param = new { BoardgameId = boardgameId };

        using var cn = DbContext.DbCxn;

        List<BoardgameContentItem> dataList = (await cn.QueryAsync<BoardgameContentItem>(sql, param)).AsList();

        return dataList;
    }
}