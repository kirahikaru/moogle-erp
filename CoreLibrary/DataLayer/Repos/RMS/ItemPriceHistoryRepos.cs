using DataLayer.Models.RMS;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.RMS;

public interface IItemPriceHistoryRepos : IBaseRepos<ItemPriceHistory>
{
	Task<ItemPriceHistory?> GetFullAsync(int id);

	Task<ItemPriceHistory?> GetItemCurrentPriceAsync(int itemId);

	Task<List<ItemPriceHistory>> GetByItemIdAsync(int itemId);

	Task<int> UpdateItemPriceAsync(ItemPriceHistory item);

	Task<List<ItemPriceHistory>> SearchAsync(
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

public class ItemPriceHistoryRepos(IDbContext dbContext) : BaseRepos<ItemPriceHistory>(dbContext, ItemPriceHistory.DatabaseObject), IItemPriceHistoryRepos
{
	public async Task<ItemPriceHistory?> GetFullAsync(int id)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t " +
                     $"LEFT JOIN {Item.MsSqlTable} i ON i.Id=t.ItemId " +
                     $"LEFT JOIN {ItemCategory.MsSqlTable} ic ON ic.Id=i.ItemCategoryId " +
                     $"WHERE t.IsDeleted=0 AND t.Id=@Id";

        using var cn = DbContext.DbCxn;

        List<ItemPriceHistory> dataList = (await cn.QueryAsync<ItemPriceHistory, Item, ItemCategory, ItemPriceHistory>(sql,
                                            (obj, item, category) =>
                                            {
                                                if (item != null)
                                                {   
                                                    item.Category = category;
                                                    obj.Item = item;
                                                }

                                                return obj;
                                            }, new { Id = id }, splitOn: "Id")).AsList();

		if (dataList != null && dataList.Count != 0)
            return dataList[0];
        else
            return null;
    }

    public async Task<List<ItemPriceHistory>> GetByItemIdAsync(int itemId)
    {
        var sql = $"SELECT * FROM {ItemPriceHistory.MsSqlTable} WHERE IsDeleted=0 AND ItemId=@ItemId";
        var param = new { ItemId = itemId };

        using var cn = DbContext.DbCxn;

        var data = (await cn.QueryAsync<ItemPriceHistory>(sql, param)).OrderByDescending(x => x.StartDateTime).ToList();
        return data;
    }

    public async Task<ItemPriceHistory?> GetItemCurrentPriceAsync(int itemId)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.ItemId=@ItemId AND t.IsCurrentPrice=1";

        using var cn = DbContext.DbCxn;

        var obj = await cn.QuerySingleOrDefaultAsync<ItemPriceHistory?>(sql, new { ItemId = itemId });
        return obj;
    }

    public async Task<int> UpdateItemPriceAsync(ItemPriceHistory obj)
    {
        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

        var currPriceQry = $"SELECT COUNT(*) FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND ItemId=@ItemId AND IsCurrentPrice=1";

        var updCurrPriceCmd = $"UPDATE {DbObject.MsSqlTable} SET IsCurrentPrice=0, EndDateTime=@EndDateTime, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime " +
                              $"WHERE IsDeleted=0 AND ItemId=@ItemId AND IsCurrentPrice=1";

        var updItemPriceCmd = $"UPDATE {Item.MsSqlTable} SET CurrencyCode=@CurrencyCode, RetailUnitPrice=@RetailUnitPrice, RetailUnitPriceKhr=@RetailUnitPriceKhr, WholeSaleUnitPrice=@WholeSaleUnitPrice, WholeSaleUnitPriceKhr=@WholeSaleUnitPriceKhr, " +
                              $"ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime " +
                              $"WHERE IsDeleted=0 AND Id=@ItemId";

        DynamicParameters updCurrPriceParam = new();
        DynamicParameters updItemPriceParam = new();

        updCurrPriceParam.Add("@ModifiedUser", obj.ModifiedUser);
        updCurrPriceParam.Add("@EndDateTime", obj.StartDateTime!.Value);
        updCurrPriceParam.Add("@ModifiedDateTime", khTimestamp);
        updCurrPriceParam.Add("@ItemId", obj.ItemId!.Value);

        updItemPriceParam.Add("@CurrencyCode", obj.CurrencyCode);
        updItemPriceParam.Add("@RetailUnitPrice", obj.RetailUnitPrice);
        updItemPriceParam.Add("@RetailUnitPriceKhr", obj.RetailUnitPriceKhr);
        updItemPriceParam.Add("@WholeSaleUnitPrice", obj.WholeSaleUnitPrice);
        updItemPriceParam.Add("@WholeSaleUnitPriceKhr", obj.WholeSaleUnitPriceKhr);
        updItemPriceParam.Add("@ModifiedUser", obj.ModifiedUser);
        updItemPriceParam.Add("@ModifiedDateTime", obj.ModifiedDateTime);
        updItemPriceParam.Add("@ItemId", obj.ItemId!.Value);

        obj.IsCurrentPrice = true;
        obj.CreatedDateTime = khTimestamp;
        obj.ModifiedDateTime = khTimestamp;

        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            int currPriceCount = await cn.ExecuteScalarAsync<int>(currPriceQry, new { ItemId = obj.ItemId!.Value }, tran);
            int updCurrPriceCount = await cn.ExecuteAsync(updCurrPriceCmd, updCurrPriceParam, tran);
            int updItemPriceCount = await cn.ExecuteAsync(updItemPriceCmd, updItemPriceParam, tran);

            if (updCurrPriceCount == currPriceCount && updItemPriceCount > 0)
            {
                int objId = await cn.InsertAsync(obj, tran);

                if (objId > 0)
                {
                    tran.Commit();
                    return objId;
                }
                else
                    throw new Exception("Failed to insert price history to database.");
            }
            else
                throw new Exception("Update failed.");
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public override async Task<List<ItemPriceHistory>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            Regex alphabets = new(@"^[a-zA-Z\s.,-]{1,}$");

            if (alphabets.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(i.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ObjectNameKh LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.String);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");

        sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT t.*, i.* FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.*, i.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<ItemPriceHistory, Item, ItemPriceHistory>(sql,
                                (itemPriceHistory, item) =>
                                {
                                    itemPriceHistory.Item = item;

                                    return itemPriceHistory;
                                }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<List<ItemPriceHistory>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        string? itemName = null,
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
            sbSql.Where("UPPER(t.ItemCode) LIKE '%'+UPPER(@ItemCode)+'%'");
            param.Add("@ItemCode", itemCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
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

        sbSql.OrderBy("i.ObjectName ASC, t.StartDateTime DESC");

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
                $"SELECT t.*, i.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<ItemPriceHistory> result = (await cn.QueryAsync<ItemPriceHistory, Item, ItemPriceHistory>(sql,
                                (itemPriceHistory, item) =>
                                {
                                    itemPriceHistory.Item = item;

                                    return itemPriceHistory;
                                }, param, splitOn: "Id")).AsList();

        return result;
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
        if (pgSize < 0)
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
            sbSql.Where("UPPER(t.ItemCode) LIKE '%'+UPPER(@ItemCode)+'%'");
            param.Add("@ItemCode", itemCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
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
}