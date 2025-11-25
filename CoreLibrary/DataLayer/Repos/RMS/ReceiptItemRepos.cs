using DataLayer.Models.RMS;

namespace DataLayer.Repos.RMS;

public interface IReceiptItemRepos : IBaseRepos<ReceiptItem>
{
    Task<List<ReceiptItem>> GetByReceiptIdAsync(int recieptId);
}

public class ReceiptItemRepos(IDbContext dbContext) : BaseRepos<ReceiptItem>(dbContext, ReceiptItem.DatabaseObject), IReceiptItemRepos
{
	public async Task<List<ReceiptItem>> GetByReceiptIdAsync(int recieptId)
    {
        var sql = $"SELECT * FROM {ReceiptItem.MsSqlTable} WHERE IsDeleted=0 AND ReceiptId=@ReceiptId";
        var param = new { ReceiptId = recieptId };

        using var cn = DbContext.DbCxn;

        List<ReceiptItem> data = (await cn.QueryAsync<ReceiptItem>(sql, param)).OrderBy(x => x.SequenceNo).ToList();
        return data;
    }
}