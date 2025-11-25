using DataLayer.Models.Library;

namespace DataLayer.Repos.Library;

public interface IBookPersonRoleMapRepos : IShellBaseRepos<BookPersonRoleMap>
{
	Task<int> BulkInsertAsync(List<BookPersonRoleMap> dataList);
	Task<int> HardDeleteByBookIdAsync(int bookId);
}

public class BookPersonRoleMapRepos(IConnectionFactory connectionFactory) : ShellBaseRepos<BookPersonRoleMap>(connectionFactory, BookPersonRoleMap.DatabaseObject), IBookPersonRoleMapRepos
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