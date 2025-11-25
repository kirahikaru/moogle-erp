using DataLayer.Models.Retail;

namespace DataLayer.Repos.Retail;

public interface IItemStockBalanceRepos : IBaseRepos<ItemStockBalance>
{
	Task<ItemStockBalance?> GetByItemAsync(int itemId);
}

public class ItemStockBalanceRepos(IConnectionFactory connectionFactory) : BaseRepos<ItemStockBalance>(connectionFactory, ItemStockBalance.DatabaseObject), IItemStockBalanceRepos
{
	public async Task<ItemStockBalance?> GetByItemAsync(int itemId)
	{
		SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.ItemId=@ItemId");
		param.Add("@ItemId", itemId);

        using var cn = ConnectionFactory.GetDbConnection()!;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		var data = await cn.QueryFirstOrDefaultAsync<ItemStockBalance>(sql, param);

		return data;
    }
}