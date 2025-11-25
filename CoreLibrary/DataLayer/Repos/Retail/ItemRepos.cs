using DataLayer.Models.HomeInventory;
using DataLayer.Models.Retail;
using DataLayer.Models.Retail.NonPersistent;
using DataLayer.Models.SystemCore.NonPersistent;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.Retail;

public interface IItemRepos : IBaseRepos<Item>
{
	Task<bool> IsDuplicateAsync(int id, string objectCode, string itemName, string barcode);

	Task<List<ItemCheckInOutHistory>> GetCheckInOutHistoryAsync(int itemId, int backDateMonthCount);

	Task<Item?> GetByBarcodeAsync(string barcode);
	Task<Item?> GetFullByBarcodeAsync(string barcode, bool includeAttachedImages = false);

	Task<int> InsertFullAsync(Item obj);
	Task<bool> UpdateFullAsync(Item obj);

	Task<List<Item>> GetByCategoryCodeAsync(string categoryCode, bool includingChildCategory = false);

	Task<bool> UpdateUnitPriceAsync(int id, string currencyCode, decimal retailUnitPrice, decimal wholeSaleUnitPrice, string modifiedUser);

	Task<Item?> GetFullAsync(int id, bool includeAttachedImages = false);

	Task<List<Item>> QuickSearch1Async(int pgSize = 0, int pgNo = 0,
		string? searchText = null,
		bool onlyHasPrice = false,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);

	Task<DataPagination> GetQuickSearch1PaginationAsync(int pgSize = 0, string? searchText = null, bool onlyHasPrice = false,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);

	Task<List<Item>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? barcode = null,
		List<string>? mfgCountryList = null,
		List<string>? categoryCodeList = null,
		string? brand = null,
		decimal? unitPriceFrom = null,
		decimal? unitPriceTo = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? barcode = null,
		List<string>? mfgCountryList = null,
		List<string>? categoryCodeList = null,
		string? brand = null,
		decimal? unitPriceFrom = null,
		decimal? unitPriceTo = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);

	Task<List<DropDownListItem>> GetForDropdownSelect1Async(string? searchText = null, int? includingObjId = null);
}

public class ItemRepos(IConnectionFactory connectionFactory) : BaseRepos<Item>(connectionFactory, Item.DatabaseObject), IItemRepos
{
	public async Task<bool> IsDuplicateAsync(int id, string objectCode, string itemName, string barcode)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id<>@Id");
        sbSql.Where("(UPPER(t.ObjectCode)=UPPER(@ObjectCode) OR UPPER(t.ObjectName)=UPPER(@ObjectName) OR t.Barcode=@Barcode)");
        param.Add("@Id", id);

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        param.Add("@ObjectName", itemName, DbType.AnsiString);
        param.Add("@Barcode", barcode, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;

        int count = await cn.ExecuteScalarAsync<int>(sql, param);

        return count > 0;
    }

    public async Task<List<Item>> GetByCategoryCodeAsync(string categoryCode, bool includingChildCategory = false)
    {
        using var cn = ConnectionFactory.GetDbConnection()!;

        if (includingChildCategory)
        {
            var itemCategoryQry = $"SELECT * FROM {ItemCategory.MsSqlTable} WHERE IsDeleted=0 AND ObjectCode=@ObjectCode";

            var itemCategory = cn.QueryFirstOrDefault<ItemCategory?>(itemCategoryQry, new DbString { Value = categoryCode, IsAnsi = true });

            if (itemCategory != null)
            {
				SqlBuilder sbSql = new();
				DynamicParameters param = new();

				sbSql.Where("t.IsDeleted=0");
                sbSql.Where("ic.HierarchyPath LIKE @ItemCategoryHierarchyPath+'%'");
                sbSql.LeftJoin($"{ItemCategory.MsSqlTable} ic ON ic.Id=t.ItemCategoryId");

                string sql = sbSql.AddTemplate($"SELECT t.* FROM {Item.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

                param.Add("@ItemCategoryHierarchyPath", itemCategory!.HierarchyPath, DbType.AnsiString);

                var data = (await cn.QueryAsync<Item>(sql, param)).AsList();
                return data;
            }
            else throw new Exception($"Cannot find this category with code '{categoryCode}' in database");
        }
        else
        {
            var sql = $"SELECT * FROM {Item.MsSqlTable} WHERE IsDeleted=0 AND ItemCategoryCode=@ItemCategoryCode";
            var param = new DbString { Value = categoryCode, IsAnsi = true };

            var data = (await cn.QueryAsync<Item>(sql, param)).ToList();
            return data;
        }
    }

    public async Task<Item?> GetFullAsync(int id, bool includeAttachedImages = false)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");

        param.Add("@Id", id);

        sbSql.LeftJoin($"{ItemCategory.MsSqlTable} tc ON tc.Id=t.ItemCategoryId");
		sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=t.MfgCountryCode");

		var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        Item? data = (await cn.QueryAsync<Item, ItemCategory, Country, Item>(sql, (item, category, mfgCountry) =>
                                    {
                                        if (item != null)
                                        {
                                            item.Category = category;
                                            item.ManufacturedCountry = mfgCountry;
                                        }
                                        
                                        return item!;
                                    }, param, splitOn: "Id")).FirstOrDefault();

        if (data is not null)
        {
			var priceHistorySql = $"SELECT * FROM {ItemPriceHistory.MsSqlTable} WHERE IsDeleted=0 AND ItemId=@ItemId Order By StartDateTime DESC";
			List<ItemPriceHistory> priceHistories = (await cn.QueryAsync<ItemPriceHistory>(priceHistorySql, new { ItemId = id })).ToList();
            data!.ItemPriceHistories = priceHistories;

			var attachedImageSql = $"SELECT * FROM {AttachedImage.MsSqlTable} WHERE IsDeleted=0 AND LinkedObjectType=@LinkedObjectType AND LinkedObjectId=@LinkedObjectId";
			if (includeAttachedImages)
            {
                List<AttachedImage> attachedImages = (await cn.QueryAsync<AttachedImage>(attachedImageSql, new { LinkedObjectType = typeof(Item).Name, LinkedObjectId = id })).ToList();
                data.Images = attachedImages;
            }

            var itemVariationSql = $"SELECT * FROM {ItemVariation.MsSqlTable} t WHERE t.ItemId=@ItemId";

            data.Variations = (await cn.QueryAsync<ItemVariation>(itemVariationSql, new { ItemId = id })).AsList();

            return data;
        }
        else
            return null;
    }

    public async Task<List<ItemCheckInOutHistory>> GetCheckInOutHistoryAsync(int itemId, int backDateMonthCount)
    {
        using var cn = ConnectionFactory.GetDbConnection()!;
        string sql = $"EXEC {ItemCheckInOutHistory.StoreProcedureName} @ItemId, @BackDateMonthCount";
        DynamicParameters param = new();
        param.Add("@ItemId", itemId);
        param.Add("@BackDateMonthCount", backDateMonthCount);
        var dataList = (await cn.QueryAsync<ItemCheckInOutHistory>(sql, param)).OrderByDescending(x => x.TransactionDateTime).AsList();

        return dataList;
    }

    public async Task<int> InsertFullAsync(Item obj)
    {
        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            DateTime khTimestamp = DateTime.UtcNow.AddHours(7);
            obj.CreatedDateTime = khTimestamp;
            obj.ModifiedDateTime = khTimestamp;

            int objId = await cn.InsertAsync(obj, tran);

            if (objId <= 0) throw new Exception("Failed to insert obj into database");

            // Insert price history
            if (obj.RetailUnitPrice is not null || obj.RetailUnitPriceKhr is not null || obj.WholeSaleUnitPrice is not null || obj.WholeSaleUnitPriceKhr is not null)
            {
                ItemPriceHistory iph = new()
                {
                    IsCurrentPrice = true,
                    CurrencyCode = obj.CurrencyCode,
                    RetailUnitPrice = obj.RetailUnitPrice,
                    RetailUnitPriceKhr = obj.RetailUnitPriceKhr,
                    WholeSaleUnitPrice = obj.WholeSaleUnitPrice,
                    WholeSaleUnitPriceKhr = obj.WholeSaleUnitPriceKhr,
                    ItemId = objId,
                    ItemCode = obj.ObjectCode,
                    Barcode = obj.Barcode,
                    StartDateTime = obj.CreatedDateTime,
                    CreatedUser = obj.CreatedUser,
                    CreatedDateTime = obj.CreatedDateTime,
                    ModifiedUser = obj.ModifiedUser,
                    ModifiedDateTime = obj.ModifiedDateTime
                };

                int iphId = await cn.InsertAsync(iph, tran);
            }

            foreach (ItemVariation variation in obj.Variations)
            {
                if (variation.IsDeleted) continue;
                if (variation.Id > 0) throw new Exception("ItemVariation cannot be existing when inserting full.");

                variation.ItemId = objId;
                variation.CreatedUser = obj.CreatedUser;
                variation.CreatedDateTime = obj.CreatedDateTime;
                variation.ModifiedUser = obj.ModifiedUser;
                variation.ModifiedDateTime = obj.ModifiedDateTime;

                int itemVariationId = await cn.InsertAsync(variation, tran);

                if (itemVariationId > 0 && variation.HasPrice)
                {
                    ItemPriceHistory iph = new()
                    {
                        IsCurrentPrice = true,
                        CurrencyCode = variation.CurrencyCode,
                        RetailUnitPrice = variation.RetailUnitPrice,
                        RetailUnitPriceKhr = variation.RetailUnitPriceKhr,
                        WholeSaleUnitPrice = variation.WholeSaleUnitPrice,
                        WholeSaleUnitPriceKhr = variation.WholeSaleUnitPriceKhr,
                        ItemId = objId,
                        ItemCode = obj.ObjectCode,
                        Barcode = variation.Barcode,
                        StartDateTime = variation.CreatedDateTime,
                        CreatedUser = variation.CreatedUser,
                        CreatedDateTime = variation.CreatedDateTime,
                        ModifiedUser = variation.ModifiedUser,
                        ModifiedDateTime = variation.ModifiedDateTime
                    };

                    int iphId = await cn.InsertAsync(iph, tran);
                }
            }

            tran.Commit();
            return objId;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateFullAsync(Item obj)
    {
        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            DateTime khTimestamp = DateTime.UtcNow.AddHours(7);
            obj.ModifiedDateTime = khTimestamp;

            bool isUpdated = await cn.UpdateAsync(obj, tran);

            if (!isUpdated) throw new Exception("Failed to update obj into database");

            foreach (ItemVariation variation in obj.Variations)
            {
                if (variation.Id > 0)
                {
                    variation.ModifiedUser = obj.ModifiedUser;
                    variation.ModifiedDateTime = obj.ModifiedDateTime;

                    bool isVarUpd = await cn.UpdateAsync(variation, tran);
                }
                else if (!variation.IsDeleted)
                {
                    variation.ItemId = obj.Id;
                    variation.CreatedUser = obj.ModifiedUser;
                    variation.CreatedDateTime = obj.ModifiedDateTime;
                    variation.ModifiedUser = obj.ModifiedUser;
                    variation.ModifiedDateTime = obj.ModifiedDateTime;

                    int itemVariationId = await cn.InsertAsync(variation, tran);

                    if (itemVariationId > 0 && variation.HasPrice)
                    {
                        ItemPriceHistory iph = new()
                        {
                            IsCurrentPrice = true,
                            CurrencyCode = variation.CurrencyCode,
                            RetailUnitPrice = variation.RetailUnitPrice,
                            RetailUnitPriceKhr = variation.RetailUnitPriceKhr,
                            WholeSaleUnitPrice = variation.WholeSaleUnitPrice,
                            WholeSaleUnitPriceKhr = variation.WholeSaleUnitPriceKhr,
                            
                            ItemId = variation.ItemId,
                            ItemCode = obj.ObjectCode,
                            Barcode = variation.Barcode,
                            StartDateTime = variation.CreatedDateTime,
                            CreatedUser = variation.CreatedUser,
                            CreatedDateTime = variation.CreatedDateTime,
                            ModifiedUser = variation.ModifiedUser,
                            ModifiedDateTime = variation.ModifiedDateTime
                        };

                        int iphId = await cn.InsertAsync(iph, tran);
                    }
                }
            }

            tran.Commit();
            return isUpdated;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<Item?> GetFullByBarcodeAsync(string barcode, bool includeAttachedImages = false)
    {
        var sql = $"SELECT * FROM {DbObject.MsSqlTable} t " +
                  $"LEFT JOIN {ItemCategory.MsSqlTable} tc ON tc.Id=t.ItemCategoryId " +
                  $"LEFT JOIN {Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=t.MfgCountryCode " +
                  $"LEFT JOIN {UnitOfMeasure.MsSqlTable} uom ON uom.IsDeleted=0 AND uom.ObjectCode=t.UnitCode " +
                  $"WHERE t.IsDeleted=0 AND t.Barcode=@Barcode;";

        DynamicParameters param = new();
        param.Add("@Barcode", barcode, DbType.AnsiString);

        var priceHistorySql = $"SELECT * FROM {ItemPriceHistory.MsSqlTable} WHERE IsDeleted=0 AND ItemId=@ItemId Order By StartDateTime DESC";
        var attachedImageSql = $"SELECT * FROM {AttachedImage.MsSqlTable} WHERE IsDeleted=0 AND LinkedObjectType=@LinkedObjectType AND LinkedObjectId=@LinkedObjectId";

        using var cn = ConnectionFactory.GetDbConnection()!;

        Item? obj = (await cn.QueryAsync<Item?, ItemCategory, Country, UnitOfMeasure, Item?>(sql, (item, category, mfgCountry, uom) =>
        {
            if (item != null)
            {
                item.Category = category;
                item.ManufacturedCountry = mfgCountry;
                item.ProductUnit = uom;
            }

            return item;
        }, param, splitOn: "Id")).AsList().SingleOrDefault();

        //if (obj != null)
        //{
        //    List<ItemPriceHistory> priceHistories = (await cn.QueryAsync<ItemPriceHistory>(priceHistorySql, new { ItemId = obj.Id })).ToList();
        //    obj.ItemPriceHistories = priceHistories;

        //    if (includeAttachedImages)
        //    {
        //        List<AttachedImage> attachedImages = (await cn.QueryAsync<AttachedImage>(attachedImageSql, new { LinkedObjectType = typeof(Item).Name, LinkedObjectId = id })).ToList();
        //        obj.Images = attachedImages;
        //    }
        //}

        return obj;
    }

    public async Task<bool> UpdateUnitPriceAsync(int id, string currencyCode, decimal retailUnitPrice, decimal wholeSaleUnitPrice, string modifiedUser)
    {
        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);
        
        var sql = $"UPDATE {Item.MsSqlTable} SET CurrencyCode=@CurrencyCode, RetailUnitPrice=@RetailUnitPrice, WholeSaleUnitPrice=@WholeSaleUnitPrice, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime WHERE IsDeleted=0 AND Id=@Id";
        var updItemHistSql = $"UPDATE {ItemPriceHistory.MsSqlTable} SET IsCurrentPrice=0, EndDateTime=@EndDateTime WHERE IsDeleted=0 AND ItemId=@Id";

        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        Item item = cn.Get<Item>(id);

        if (item == null)
            throw new Exception($"Item (id={id}) not found in database.");

        ItemPriceHistory priceHist = new()
        {
            ItemId = item.Id,
            ItemCode = item.ObjectCode,
            Barcode = item.Barcode,
            CurrencyCode = currencyCode,
            IsCurrentPrice = true,
            RetailUnitPrice = retailUnitPrice,
            WholeSaleUnitPrice = wholeSaleUnitPrice,
            StartDateTime = khTimestamp,
            CreatedDateTime = khTimestamp,
            CreatedUser = modifiedUser,
            ModifiedDateTime = khTimestamp,
            ModifiedUser = modifiedUser
        };

        using var tran = cn.BeginTransaction();
        try
        {
            var updItemHistParam = new { EndDateTime = khTimestamp, Id = id };
            int updItemHistCount = await cn.ExecuteAsync(updItemHistSql, updItemHistParam, tran);

            if (updItemHistCount > 0)
            {
                int itemPriceHistId = await cn.InsertAsync(priceHist, tran);

                if (itemPriceHistId > 0)
                {
                    var mainParam = new { CurrencyCode = currencyCode, RetailUnitPrice = retailUnitPrice, WholeSaleUnitPrice = wholeSaleUnitPrice, ModifiedUser = modifiedUser, ModifiedDateTime = khTimestamp, Id = id };
                    int updCount = await cn.ExecuteAsync(sql, mainParam, tran);
                }
                else throw new Exception("Failed to insert latest item price history");
            }
            else throw new Exception("Failed to update Item Price History's current price to non current.");

            tran.Commit();
            return true;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<Item?> GetByBarcodeAsync(string barcode)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Barcode=@Barcode");

        sbSql.LeftJoin($"{ItemCategory.MsSqlTable} tc ON tc.Id=t.ItemCategoryId");
        sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=t.MfgCountryCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} uom ON uom.IsDeleted=0 AND uom.ObjectCode=t.UnitCode");

        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        var param = new { Barcode = new DbString { Value = barcode, IsAnsi = true } };

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<Item, ItemCategory, Country, UnitOfMeasure, Item>(sql, (item, ctg, mfgCty, uom) =>
        {
            item.Category = ctg;
            item.ManufacturedCountry = mfgCty;
            item.ProductUnit = uom;

            return item;
        }, param, splitOn: "Id")).FirstOrDefault();

        return data;
    }

	public override async Task<KeyValuePair<int, IEnumerable<Item>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null
	)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (Regex.IsMatch(searchText, @"$\d{5,}^"))
            {
                sbSql.Where("t.Barcode=@SearchText");
                param.Add("@SearchText", searchText, DbType.AnsiString);
			}
            else
            {
                sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count != 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		sbSql.LeftJoin($"{ItemCategory.MsSqlTable} ic ON ic.Id=t.ItemCategoryId");
		sbSql.LeftJoin($"{Country.MsSqlTable} mc ON mc.IsDeleted=0 AND mc.ObjectCode=t.MfgCountryCode");

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
			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = await cn.QueryAsync<Item, ItemCategory, Country, Item>(sql,
										(item, ctg, cty) =>
										{
                                            item.Category = ctg;
                                            item.ManufacturedCountry = cty;
											return item;
										}, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public async Task<List<Item>> QuickSearch1Async(int pgSize = 0, int pgNo = 0, string? searchText = null, bool onlyHasPrice = false,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        if (onlyHasPrice)
        {
            sbSql.Where("t.RetailUnitPrice IS NOT NULL");
            sbSql.Where("t.RetailUnitPrice > 0");
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            Regex alphabets = new(@"^[a-zA-Z ]{1,}$");
            Regex numbers = new(@"^[0-9\-]{1,}$");

            if (searchText.Length >= 5 && (searchText.StartsWith("ctg:") || searchText.StartsWith("cat:")))
            {
                sbSql.Where("LOWER(ic.ObjectName) LIKE '%'+LOWER(@CategoryName)+'%'");
                param.Add("@CategoryName", searchText.Replace("ctg:", "", StringComparison.OrdinalIgnoreCase).Replace("cat:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (alphabets.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%' OR UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else if (numbers.IsMatch(searchText))
            {
                sbSql.Where("(t.Barcode LIKE '%'+@SearchText+'%' OR t.[UPC] LIKE '%'+@SearchText+'%' OR t.[EAN] LIKE '%'+@SearchText+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ObjectNameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText, DbType.String);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "ObjectNameKh":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.ObjectNameKh IS NOT NULL");
                        break;

                    case "RetailUnitPrice":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.RetailUnitPrice IS NOT NULL");
                        break;

                    case "MfgCountryCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.MfgCountryCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }

        sbSql.LeftJoin($"{ItemCategory.MsSqlTable} ic ON ic.Id=t.ItemCategoryId");
        sbSql.LeftJoin($"{Country.MsSqlTable} mc ON mc.IsDeleted=0 AND mc.ObjectCode=t.MfgCountryCode");

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

            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<Item> result = (await cn.QueryAsync<Item, ItemCategory, Country, Item>(sql,
                                (item, itemCategory, mfgCountry) =>
                                {
                                    item.Category = itemCategory;
                                    item.ManufacturedCountry = mfgCountry;

                                    return item;
                                }, param, splitOn: "Id")).AsList();

        return result;
    }

    public async Task<DataPagination> GetQuickSearch1PaginationAsync(int pgSize = 0, string? searchText = null, bool onlyHasPrice = false,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        if (onlyHasPrice)
        {
            sbSql.Where("t.RetailUnitPrice IS NOT NULL");
            sbSql.Where("t.RetailUnitPrice > 0");
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            Regex alphabets = new(@"^[a-zA-Z ]{1,}$");
            Regex numbers = new(@"^[0-9\-]{1,}$");

            if (searchText.Length >= 5 && (searchText.StartsWith("ctg:") || searchText.StartsWith("cat:")))
            {
                sbSql.Where("LOWER(ic.ObjectName) LIKE '%'+@CategoryName+'%'");
                param.Add("@CategoryName", searchText.Replace("ctg:", "", StringComparison.OrdinalIgnoreCase).Replace("cat:", "", StringComparison.OrdinalIgnoreCase).ToLower(), DbType.AnsiString);
            }
            else if (alphabets.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%' OR UPPER(t.ObjectName) LIKE '%'+@SearchText+'%')");
                param.Add("@SearchText", searchText.ToUpper(), DbType.AnsiString);
            }
            else if (numbers.IsMatch(searchText))
            {
                sbSql.Where("(t.Barcode LIKE '%'+@SearchText+'%' OR t.[UPC] LIKE '%'+@SearchText+'%' OR t.[EAN] LIKE '%'+@SearchText+'%')");
                param.Add("@SearchText", searchText.ToUpper(), DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ObjectNameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText.ToUpper(), DbType.String);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "ObjectNameKh":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.ObjectNameKh IS NOT NULL");
                        break;

                    case "RetailUnitPrice":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.RetailUnitPrice IS NOT NULL");
                        break;

                    case "MfgCountryCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.MfgCountryCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

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

    public override async Task<List<Item>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            Regex alphabets = new(@"^[a-zA-Z ]{1,}$");
            Regex numbers = new(@"^[0-9\-]{1,}$");

            if (searchText.Length>=5 && (searchText.StartsWith("ctg:") || searchText.StartsWith("cat:")))
            {
                sbSql.Where("LOWER(ic.ObjectName) LIKE '%'+@CategoryName+'%'");
                param.Add("@CategoryName", searchText.Replace("ctg:", "", StringComparison.OrdinalIgnoreCase).Replace("cat:", "", StringComparison.OrdinalIgnoreCase).ToLower(), DbType.AnsiString);
            }
            else if (alphabets.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%' OR UPPER(t.ObjectName) LIKE '%'+@SearchText+'%')");
                param.Add("@SearchText", searchText.ToUpper(), DbType.AnsiString);
            }
            else if (numbers.IsMatch(searchText))
            {
                sbSql.Where("(t.Barcode LIKE '%'+@SearchText+'%' OR t.[UPC] LIKE '%'+@SearchText+'%' OR t.[EAN] LIKE '%'+@SearchText+'%')");
                param.Add("@SearchText", searchText.ToUpper(), DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ObjectNameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText.ToUpper(), DbType.String);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{ItemCategory.MsSqlTable} ic ON ic.Id=t.ItemCategoryId");
        sbSql.LeftJoin($"{Country.MsSqlTable} mc ON mc.IsDeleted=0 AND mc.ObjectCode=t.MfgCountryCode");

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

            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<Item, ItemCategory, Country, Item>(sql, 
                                (item, itemCategory, mfgCountry) =>
                                {
                                    item.Category = itemCategory;
                                    item.ManufacturedCountry = mfgCountry;

                                    return item;
                                }, param, splitOn: "Id")).AsList();

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
            Regex alphabets = new(@"^[a-zA-Z ]{1,}$");
            Regex numbers = new(@"^[0-9\-]{1,}$");

            if (searchText.Length >= 5 && (searchText.StartsWith("ctg:") || searchText.StartsWith("cat:")))
            {
                sbSql.Where("LOWER(ic.ObjectName) LIKE '%'+@CategoryName+'%'");
                param.Add("@CategoryName", searchText.Replace("ctg:", "", StringComparison.OrdinalIgnoreCase).Replace("cat:", "", StringComparison.OrdinalIgnoreCase).ToLower(), DbType.AnsiString);
            }
            else if (alphabets.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%' OR UPPER(t.ObjectName) LIKE '%'+@SearchText+'%')");
                param.Add("@SearchText", searchText.ToUpper(), DbType.AnsiString);
            }
            else if (numbers.IsMatch(searchText))
            {
                sbSql.Where("(t.Barcode LIKE '%'+@SearchText+'%' OR t.[UPC] LIKE '%'+@SearchText+'%' OR t.[EAN] LIKE '%'+@SearchText+'%')");
                param.Add("@SearchText", searchText.ToUpper(), DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ObjectNameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText.ToUpper(), DbType.String);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

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

    public async Task<List<Item>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? barcode = null,
        List<string>? mfgCountryList = null,
        List<string>? categoryCodeList = null,
        string? brand = null,
        decimal? unitPriceFrom = null,
        decimal? unitPriceTo = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(brand))
        {
            sbSql.Where("UPPER(t.Brand) LIKE @Brand+'%'");
            param.Add("@Brand", brand.ToUpper(), DbType.AnsiString);
        }

        if (mfgCountryList != null && mfgCountryList.Count != 0)
        {
            if (mfgCountryList.Count == 1)
            {
                sbSql.Where("t.ManufacturedCountryCode=@ManufacturedCountryCode");
                param.Add("@ManufacturedCountryCode", mfgCountryList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ManufacturedCountryCode IN @ManufacturedCountryCodeList");
                param.Add("@ManufacturedCountryCodeList", mfgCountryList);
            }
        }

        if (unitPriceFrom.HasValue)
        {
            sbSql.Where("t.UnitPrice IS NOT NULL AND t.UnitPrice>=@UnitPriceFrom");

            param.Add("@UnitPriceFrom", unitPriceFrom.Value);

            if (unitPriceTo.HasValue)
            {
                sbSql.Where("t.UnitPrice<=@UnitPriceTo");
                param.Add("@UnitPriceTo", unitPriceTo.Value);
            }
        }
        else if (unitPriceTo.HasValue)
        {
            sbSql.Where("t.UnitPrice IS NOT NULL AND t.UnitPrice<=@UnitPriceTo");
            param.Add("@UnitPriceTo", unitPriceTo.Value);
        }

        if (categoryCodeList != null && categoryCodeList.Count != 0)
        {
            if (categoryCodeList.Count == 1)
            {
                sbSql.Where("t.ItemCategoryCode=@CategoryCode");
                param.Add("@CategoryCode", categoryCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ItemCategoryCode IN @CategoryCodeList");
                param.Add("@CategoryCodeList", categoryCodeList);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "ObjectNameKh":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.ObjectNameKh IS NOT NULL");
                        break;

                    case "RetailUnitPrice":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.RetailUnitPrice IS NOT NULL");
                        break;

                    case "MfgCountryCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.MfgCountryCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }
        #endregion

        sbSql.LeftJoin($"{ItemCategory.MsSqlTable} ic ON ic.Id=t.ItemCategoryId");
        sbSql.LeftJoin($"{Country.MsSqlTable} mc ON mc.IsDeleted=0 AND mc.ObjectCode=t.MfgCountryCode");

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

            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<Item, ItemCategory, Country, Item>(sql,
                                (item, itemCategory, mfgCountry) =>
                                {
                                    item.Category = itemCategory;
                                    item.ManufacturedCountry = mfgCountry;

                                    return item;
                                }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? barcode = null,
        List<string>? mfgCountryList = null,
        List<string>? categoryCodeList = null,
        string? brand = null,
        decimal? unitPriceFrom = null,
        decimal? unitPriceTo = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(brand))
        {
            sbSql.Where("UPPER(t.Brand) LIKE @Brand+'%'");
            param.Add("@Brand", brand.ToUpper(), DbType.AnsiString);
        }

        if (mfgCountryList != null && mfgCountryList.Any())
        {
            if (mfgCountryList.Count == 1)
            {
                sbSql.Where("t.ManufacturedCountryCode=@ManufacturedCountryCode");
                param.Add("@ManufacturedCountryCode", mfgCountryList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ManufacturedCountryCode IN @ManufacturedCountryCodeList");
                param.Add("@ManufacturedCountryCodeList", mfgCountryList);
            }
        }

        if (unitPriceFrom.HasValue)
        {
            sbSql.Where("t.UnitPrice IS NOT NULL AND t.UnitPrice>=@UnitPriceFrom");

            param.Add("@UnitPriceFrom", unitPriceFrom.Value);

            if (unitPriceTo.HasValue)
            {
                sbSql.Where("t.UnitPrice<=@UnitPriceTo");
                param.Add("@UnitPriceTo", unitPriceTo.Value);
            }
        }
        else if (unitPriceTo.HasValue)
        {
            sbSql.Where("t.UnitPrice IS NOT NULL AND t.UnitPrice<=@UnitPriceTo");
            param.Add("@UnitPriceTo", unitPriceTo.Value);
        }

        if (categoryCodeList != null && categoryCodeList.Count != 0)
        {
            if (categoryCodeList.Count == 1)
            {
                sbSql.Where("t.ItemCategoryCode=@CategoryCode");
                param.Add("@CategoryCode", categoryCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ItemCategoryCode IN @CategoryCodeList");
                param.Add("@CategoryCodeList", categoryCodeList);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "ObjectNameKh":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.ObjectNameKh IS NOT NULL");
                        break;

                    case "RetailUnitPrice":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.RetailUnitPrice IS NOT NULL");
                        break;

                    case "MfgCountryCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.MfgCountryCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

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

    public async Task<List<DropDownListItem>> GetForDropdownSelect1Async(string? searchText = null, int? includingObjId = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Select("t.Id")
            .Select("'ObjectId'=t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("'ObjectNameEn'=t.ObjectName")
            .Select("t.ObjectNameKh");

        sbSql.Where("t.IsDeleted=0");

        string sql;

        if (includingObjId != null)
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                sbSql.Where("(LOWER(t.ObjectName) LIKE '%'+LOWER(@SearchText)+'%' OR t.Barcode=@SearchText)");
                param.Add("@SearchText", searchText);
            }

            sql = sbSql.AddTemplate($"SELECT TOP 100 /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ UNION SELECT /**select**/ FROM {DbObject.MsSqlTable} t WHERE t.Id=@IncludingObjId").RawSql;
            param.Add("@IncludingObjId", includingObjId!.Value);
        }
        else
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                sbSql.Where("(LOWER(t.ObjectName) LIKE '%'+LOWER(@SearchText)+'%' OR t.Barcode=@SearchText)");
                param.Add("@SearchText", searchText);
            }

            sql = sbSql.AddTemplate($"SELECT TOP 100 /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;
        var dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).OrderBy(x => x.ObjectName).AsList();

        return dataList;
    }
}