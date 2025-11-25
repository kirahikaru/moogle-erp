using DataLayer.Models.RMS;
using MongoDB.Driver;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.RMS;

public interface IItemVariationRepos : IBaseRepos<ItemVariation>
{
	Task<ItemVariation?> GetFullAsync(int id);

	Task<List<ItemVariation>> GetByItemIdAsync(int itemId);

	Task<bool> IsDuplicateCodeAsync(int objId, string objectCode, string itemObjectCode);

	Task<bool> IsDuplicateCodeAsync(int objId, string objectCode, int itemId);

	Task<List<ItemVariation>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? itemName = null,
		string? itemCode = null,
		string? barcode = null,
		decimal? retailPriceFrom = null,
		decimal? retailPriceTo = null,
		decimal? wholeSalePriceFrom = null,
		decimal? wholeSalePriceTo = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? itemName = null,
		string? itemCode = null,
		string? barcode = null,
		decimal? retailPriceFrom = null,
		decimal? retailPriceTo = null,
		decimal? wholeSalePriceFrom = null,
		decimal? wholeSalePriceTo = null);
}

public class ItemVariationRepos(IDbContext dbContext) : BaseRepos<ItemVariation>(dbContext, ItemVariation.DatabaseObject), IItemVariationRepos
{
	public async Task<bool> IsDuplicateCodeAsync(int objId, string objectCode, string itemObjectCode)
	{
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

		param.Add("@Id", objId);
		param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		param.Add("@ItemObjectCode", itemObjectCode, DbType.AnsiString);

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id<>@Id");
        sbSql.Where("t.ObjectCode=@ObjectCode");
		sbSql.Where("i.ObjectCode=@ItemObjectCode");

		sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");

        using var cn = DbContext.DbCxn;

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(sql, param);
		return count > 0;
    }

    public async Task<bool> IsDuplicateCodeAsync(int objId, string objectCode, int itemId)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        param.Add("@Id", objId);
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        param.Add("@ItemId", itemId);

        sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id<>@Id");
        sbSql.Where("t.ObjectCode=@ObjectCode");
        sbSql.Where("t.ItemId=@ItemId");

        using var cn = DbContext.DbCxn;

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        int count = await cn.ExecuteScalarAsync<int>(sql, param);
        return count > 0;
    }

    public async Task<List<ItemVariation>> GetByItemIdAsync(int itemId)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.ItemId=@ItemId");

		param.Add("@ItemId", itemId);

		using var cn = DbContext.DbCxn;

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		var dataList = (await cn.QueryAsync<ItemVariation>(sql, param)).AsList();

		return dataList;
	}

	public async Task<ItemVariation?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");

		sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
		sbSql.LeftJoin($"{ItemCategory.MsSqlTable} ic ON ic.Id=i.ItemCategoryId");

		param.Add("@Id", id);

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		using var cn = DbContext.DbCxn;

		var data = (await cn.QueryAsync<ItemVariation, Item, ItemCategory, ItemVariation>(sql,
											(obj, item, category) =>
											{
												if (item != null)
												{
													item.Category = category;
													obj.Item = item;
												}

												return obj;
											}, new { Id = id }, splitOn: "Id")).FirstOrDefault();

		return data;
	}

	public override async Task<List<ItemVariation>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("UPPER(i.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else if (searchText.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("UPPER(i.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("code:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("UPPER(i.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.OrderBy("i.ObjectName ASC");

		sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
		sbSql.LeftJoin($"{ItemCategory.MsSqlTable} ic ON ic.Id=i.ItemCategoryId");

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT t.*, i.*, ic.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<ItemVariation, Item, ItemCategory, ItemVariation>(sql, 
								(obj, item, category) => 
								{
									if (item != null)
										item.Category = category;

									obj.Item = item;

									return obj;
								}, param, splitOn:"Id")).AsList();

		return dataList;
	}

	public override async Task<DataPagination> GetQuickSearchPaginationAsync(int pgSize = 0, string? searchText = null, List<int>? excludeIdList = null)
	{
		if (pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("UPPER(i.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else if (searchText.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("UPPER(i.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("code:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("UPPER(i.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.OrderBy("i.ObjectName ASC");

		sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
		sbSql.LeftJoin($"{ItemCategory.MsSqlTable} ic ON ic.Id=i.ItemCategoryId");

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		using var cn = DbContext.DbCxn;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

		DataPagination pagination = new()
		{
			ObjectType = typeof(ItemVariation).Name,
			PageSize = pgSize,
			PageCount = pageCount,
			RecordCount = (int)recordCount
		};

		return pagination;
	}

	public async Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0, 
		string? itemName = null, 
		string? itemCode = null, 
		string? barcode = null, 
		decimal? retailPriceFrom = null, 
		decimal? retailPriceTo = null, 
		decimal? wholeSalePriceFrom = null, 
		decimal? wholeSalePriceTo = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(itemName))
		{
			sbSql.Where("UPPER(i.ObjectName) LIKE @UPPER(ObjectName)+'%'");
			param.Add("@ObjectName", itemName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(itemCode))
		{
			sbSql.Where("UPPER(i.ObjectCode) LIKE '%'+UPPER(@ItemCode)+'%'");
			param.Add("@ItemCode", itemCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(barcode))
		{
			sbSql.Where("i.Barcode LIKE '%'+@Barcode+'%'");
			param.Add("@Barcode", barcode, DbType.AnsiString);
		}

		if (retailPriceFrom.HasValue)
		{
			sbSql.Where("t.RetailUnitPrice IS NOT NULL AND t.RetailUnitPrice >= @RetailPriceFrom");
			param.Add("@RetailPriceFrom", retailPriceFrom!.Value);

			if (retailPriceTo.HasValue)
			{
				sbSql.Where("t.RetailUnitPrice <= @RetailPriceTo");
				param.Add("@RetailPriceTo", retailPriceTo!.Value);
			}
		}
		else if (retailPriceTo.HasValue)
		{
			sbSql.Where("t.RetailUnitPrice IS NOT NULL AND t.RetailUnitPrice <= @RetailPriceTo");
			param.Add("@RetailPriceTo", retailPriceTo!.Value);
		}

		if (wholeSalePriceFrom.HasValue)
		{
			sbSql.Where("t.WholeSaleUnitPrice IS NOT NULL AND t.WholeSaleUnitPrice >= @WholeSalePriceFrom");
			param.Add("@WholeSalePriceFrom", wholeSalePriceFrom!.Value);

			if (wholeSalePriceTo.HasValue)
			{
				sbSql.Where("t.RetailUnitPrice <= @RetailPriceTo");
				param.Add("@RetailPriceTo", wholeSalePriceTo!.Value);
			}
		}
		else if (retailPriceTo.HasValue)
		{
			sbSql.Where("t.RetailUnitPrice IS NOT NULL AND t.RetailUnitPrice <= @RetailPriceTo");
			param.Add("@RetailPriceTo", retailPriceTo!.Value);
		}
		#endregion

		sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
		sbSql.LeftJoin($"{ItemCategory.MsSqlTable} ic ON ic.Id=i.ItemCategoryId");

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		using var cn = DbContext.DbCxn;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

		DataPagination pagination = new()
		{
			ObjectType = typeof(Item).Name,
			PageSize = pgSize,
			PageCount = pageCount,
			RecordCount = (int)recordCount
		};

		return pagination;
	}

	public async Task<List<ItemVariation>> SearchAsync(
		int pgSize = 0, 
		int pgNo = 0, string? itemName = null,
		string? itemCode = null,
		string? barcode = null,
		decimal? retailPriceFrom = null,
		decimal? retailPriceTo = null,
		decimal? wholeSalePriceFrom = null,
		decimal? wholeSalePriceTo = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(itemName))
		{
			sbSql.Where("UPPER(i.ObjectName) LIKE @UPPER(ObjectName)+'%'");
			param.Add("@ObjectName", itemName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(itemCode))
		{
			sbSql.Where("UPPER(i.ObjectCode) LIKE '%'+UPPER(@ItemCode)+'%'");
			param.Add("@ItemCode", itemCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(barcode))
		{
			sbSql.Where("i.Barcode LIKE '%'+@Barcode+'%'");
			param.Add("@Barcode", barcode, DbType.AnsiString);
		}

		if (retailPriceFrom.HasValue)
		{
			sbSql.Where("t.RetailUnitPrice IS NOT NULL AND t.RetailUnitPrice >= @RetailPriceFrom");
			param.Add("@RetailPriceFrom", retailPriceFrom!.Value);

			if (retailPriceTo.HasValue)
			{
				sbSql.Where("t.RetailUnitPrice <= @RetailPriceTo");
				param.Add("@RetailPriceTo", retailPriceTo!.Value);
			}
		}
		else if (retailPriceTo.HasValue)
		{
			sbSql.Where("t.RetailUnitPrice IS NOT NULL AND t.RetailUnitPrice <= @RetailPriceTo");
			param.Add("@RetailPriceTo", retailPriceTo!.Value);
		}

		if (wholeSalePriceFrom.HasValue)
		{
			sbSql.Where("t.WholeSaleUnitPrice IS NOT NULL AND t.WholeSaleUnitPrice >= @WholeSalePriceFrom");
			param.Add("@WholeSalePriceFrom", wholeSalePriceFrom!.Value);

			if (wholeSalePriceTo.HasValue)
			{
				sbSql.Where("t.RetailUnitPrice <= @RetailPriceTo");
				param.Add("@RetailPriceTo", wholeSalePriceTo!.Value);
			}
		}
		else if (retailPriceTo.HasValue)
		{
			sbSql.Where("t.RetailUnitPrice IS NOT NULL AND t.RetailUnitPrice <= @RetailPriceTo");
			param.Add("@RetailPriceTo", retailPriceTo!.Value);
		}
		#endregion

		sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
		sbSql.LeftJoin($"{ItemCategory.MsSqlTable} ic ON ic.Id=i.ItemCategoryId");

		sbSql.OrderBy("i.ObjectName ASC");

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t LEFT JOIN {Item.MsSqlTable} i ON i.Id=t.ItemId /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT t.*, i.*, ic.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<ItemVariation, Item, ItemCategory, ItemVariation>(sql,
								(obj, item, category) =>
								{
									if (item != null)
										item.Category = category;

									obj.Item = item;

									return obj;
								}, param, splitOn: "Id")).AsList();

		return dataList;
	}
}