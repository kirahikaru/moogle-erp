using DataLayer.Models.RMS;
using MongoDB.Driver;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.RMS;

public interface IItemSupplierRepos : IBaseRepos<ItemSupplier>
{
	Task<ItemSupplier?> GetFullAsync(int id);

	Task<bool> HasExistingAsync(int itemId, int supplierId, int objId);

	Task<List<ItemSupplier>> GetByItemAsync(int itemId);

	Task<List<ItemSupplier>> GetBySupplierAsync(int supplierId);
}

public class ItemSupplierRepos(IDbContext dbContext) : BaseRepos<ItemSupplier>(dbContext, ItemSupplier.DatabaseObject), IItemSupplierRepos
{
	public async Task<bool> HasExistingAsync(int itemId, int supplierId, int objId)
	{
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.IsCurrent=1");
        sbSql.Where("t.ItemId=@ItemId");
        sbSql.Where("t.SupplierId=@SupplierId");
        sbSql.Where("t.Id<>@Id");

		param.Add("@ItemId", itemId);
        param.Add("@SupplierId", supplierId);
        param.Add("@Id", objId);

        using var cn = DbContext.DbCxn;

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(sql, param);

		return count > 0;
    }

    public async Task<ItemSupplier?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");

		sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
		sbSql.LeftJoin($"{ItemCategory.MsSqlTable} tc ON tc.Id=i.ItemCategoryId");
		sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=i.MfgCountryCode");
		sbSql.LeftJoin($"{Supplier.MsSqlTable} s ON s.Id=t.SupplierId");

		param.Add("@Id", id);

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		using var cn = DbContext.DbCxn;

		var data = (await cn.QueryAsync<ItemSupplier, Item, ItemCategory, Country, Supplier, ItemSupplier>(sql,
											(obj, item, category, mfgCty, supplier) =>
											{
												if (item != null)
												{
													item.Category = category;
													item.ManufacturedCountry = mfgCty;
													obj.Item = item;
												}

												obj.Supplier = supplier;

												return obj;
											}, new { Id = id }, splitOn: "Id")).FirstOrDefault();

		return data;
	}

	public async Task<List<ItemSupplier>> GetByItemAsync(int itemId)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.ItemId=@ItemId");

		param.Add("@ItemId", itemId);

		sbSql.LeftJoin($"{Supplier.MsSqlTable} s ON s.Id=t.SupplierId");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		var dataList = (await cn.QueryAsync<ItemSupplier, Supplier, ItemSupplier>(sql,
											(obj, supplier) =>
											{
												obj.Supplier = supplier;

												return obj;
											}, param, splitOn: "Id")).AsList();

		return dataList;
	}

	public async Task<List<ItemSupplier>> GetBySupplierAsync(int supplierId)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.SupplierId=@SupplierId");

		sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
		sbSql.LeftJoin($"{ItemCategory.MsSqlTable} tc ON tc.Id=i.ItemCategoryId");
		sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=i.MfgCountryCode");

		sbSql.LeftJoin($"{Supplier.MsSqlTable} s ON s.Id=t.SupplierId");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		var dataList = (await cn.QueryAsync<ItemSupplier, Item, ItemCategory, Country, ItemSupplier>(sql,
											(obj, item, category, mfgCty) =>
											{
												if (item != null)
												{
													item.Category = category;
													item.ManufacturedCountry = mfgCty;
													obj.Item = item;
												}

												return obj;
											}, param, splitOn: "Id")).AsList();

		return dataList;
	}
}