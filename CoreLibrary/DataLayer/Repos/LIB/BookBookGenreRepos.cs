using DataLayer.Models.LIB;

namespace DataLayer.Repos.LIB;

public interface IBookBookGenreRepos : IShellBaseRepos<BookBookGenre>
{
	Task<int> BulkInsertAsync(List<BookPersonRoleMap> dataList);
	Task<int> HardDeleteByBookIdAsync(int bookId);
}

public class BookBookGenreRepos(IDbContext dbContext) : ShellBaseRepos<BookBookGenre>(dbContext, BookBookGenre.DatabaseObject), IBookBookGenreRepos
{
	public async Task<int> BulkInsertAsync(List<BookPersonRoleMap> dataList)
    {
        using var cn = DbContext.DbCxn;

        int x = await cn.InsertAsync(dataList);
        return x;
    }

    public async Task<int> HardDeleteByBookIdAsync(int bookId)
    {
        string sql = $"DELETE FROM {DbObject.MsSqlTable} WHERE BookId=@BookId";
        var param = new { BookId = bookId };

        using var cn = DbContext.DbCxn;

        int x = await cn.ExecuteAsync(sql, param);
        return x;
    }
}