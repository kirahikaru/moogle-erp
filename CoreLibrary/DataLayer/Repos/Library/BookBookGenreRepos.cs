using DataLayer.Models.Library;

namespace DataLayer.Repos.Library;

public interface IBookBookGenreRepos : IShellBaseRepos<BookBookGenre>
{
	Task<int> BulkInsertAsync(List<BookPersonRoleMap> dataList);
	Task<int> HardDeleteByBookIdAsync(int bookId);
}

public class BookBookGenreRepos(IConnectionFactory connectionFactory) : ShellBaseRepos<BookBookGenre>(connectionFactory, BookBookGenre.DatabaseObject), IBookBookGenreRepos
{
	public async Task<int> BulkInsertAsync(List<BookPersonRoleMap> dataList)
    {
        using var cn = ConnectionFactory.GetDbConnection()!;

        int x = await cn.InsertAsync(dataList);
        return x;
    }

    public async Task<int> HardDeleteByBookIdAsync(int bookId)
    {
        string sql = $"DELETE FROM {DbObject.MsSqlTable} WHERE BookId=@BookId";
        var param = new { BookId = bookId };

        using var cn = ConnectionFactory.GetDbConnection()!;

        int x = await cn.ExecuteAsync(sql, param);
        return x;
    }
}