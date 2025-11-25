using DataLayer.Models.Retail;
using DataLayer.Models.Finance;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.Retail;

public interface IInventoryCheckInItemRepos : IBaseRepos<InventoryCheckInItem>
{
	Task<List<InventoryCheckInItem>> GetByItemIdAsync(int itemId);
	Task<List<InventoryCheckInItem>> GetByMainObjectAsync(int inventoryCheckInId);

	Task<List<InventoryCheckInItem>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		List<int>? locationIdList = null,
		string? barcode = null,
		string? brand = null,
		string? batchID = null,
		decimal? totalAmountFrom = null,
		decimal? totalAmountTo = null,
		DateTime? mfgDateFrom = null,
		DateTime? mfgDateTo = null,
		DateTime? expiryDateFrom = null,
		DateTime? expiryDateTo = null,
		List<string>? mfgCountryCodeList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		List<int>? locationIdList = null,
		string? barcode = null,
		string? brand = null,
		string? batchID = null,
		decimal? totalAmountFrom = null,
		decimal? totalAmountTo = null,
		DateTime? mfgDateFrom = null,
		DateTime? mfgDateTo = null,
		DateTime? expiryDateFrom = null,
		DateTime? expiryDateTo = null,
		List<string>? mfgCountryCodeList = null);
}

public class InventoryCheckInItemRepos(IConnectionFactory connectionFactory) : BaseWorkflowEnabledRepos<InventoryCheckInItem>(connectionFactory, InventoryCheckInItem.DatabaseObject), IInventoryCheckInItemRepos
{
	public async Task<List<InventoryCheckInItem>> GetByItemIdAsync(int itemId)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.ItemId=@ItemId");

        sbSql.LeftJoin($"{InventoryCheckIn.MsSqlTable} ici ON ici.Id=t.InventoryCheckInId");
        sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
        sbSql.LeftJoin($"{Location.MsSqlTable} l ON l.Id=t.LocationId");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} uom ON uom.IsDeleted=0 AND uom.ObjectCode=t.UnitCode");
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.MfgCountryCode");
        sbSql.LeftJoin($"{Manufacturer.MsSqlTable} mf ON mf.Id=t.ManufacturerId");
        //sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");

        sbSql.OrderBy("t.CheckedInDateTime DESC");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;

        var param = new { ItemId = itemId };

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<InventoryCheckInItem, InventoryCheckIn, Item, Location, UnitOfMeasure, Country, Manufacturer, InventoryCheckInItem>(sql, 
                (checkInItem, checkIn, item, location, uom, country, manufacturer) => 
                {
                    checkInItem.InventoryCheckIn = checkIn;
                    checkInItem.Item = item;
                    checkInItem.Location = location;
                    checkInItem.Unit = uom;
                    checkInItem.MfgCountry = country;
                    checkInItem.Manufacturer = manufacturer;

                    return checkInItem;
                }, param, splitOn:"Id")).AsList();

        return dataList;
    }

    public async Task<List<InventoryCheckInItem>> GetByMainObjectAsync(int inventoryCheckInId)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.InventoryCheckInId=@InventoryCheckInId");

        //sbSql.LeftJoin($"{InventoryCheckIn.MsSqlTable} ici ON ici.Id=t.InventoryCheckInId");
        //sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
        sbSql.LeftJoin($"{Location.MsSqlTable} l ON l.Id=t.LocationId");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} uom ON uom.IsDeleted=0 AND uom.ObjectCode=t.UnitCode");
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.MfgCountryCode");
        sbSql.LeftJoin($"{Manufacturer.MsSqlTable} mf ON mf.Id=t.ManufacturerId");
        //sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");

        sbSql.OrderBy("t.SequenceNo ASC");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;

        var param = new { InventoryCheckInId = inventoryCheckInId };

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<InventoryCheckInItem, Location, UnitOfMeasure, Country, Manufacturer, InventoryCheckInItem>(sql,
                (checkInItem, location, uom, country, manufacturer) =>
                {
                    checkInItem.Location = location;
                    checkInItem.Unit = uom;
                    checkInItem.MfgCountry = country;
                    checkInItem.Manufacturer = manufacturer;

                    return checkInItem;
                }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public override async Task<List<InventoryCheckInItem>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("(t.ObjectName LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{InventoryCheckIn.MsSqlTable} ici ON ici.Id=t.InventoryCheckInId");
        sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
        sbSql.LeftJoin($"{Location.MsSqlTable} l ON l.Id=t.LocationId");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} uom ON uom.IsDeleted=0 AND uom.ObjectCode=t.UnitCode");
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.MfgCountryCode");
        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");

        sbSql.OrderBy("t.CheckInDateTime DESC");
        sbSql.OrderBy("t.ObjectCode DESC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                    $"SELECT t.*, ici.*, i.*, l.*, uom.*, cty.*, curr.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<InventoryCheckInItem> data = (await cn.QueryAsync<InventoryCheckInItem, InventoryCheckIn, Item, Location, UnitOfMeasure, Country, Currency, InventoryCheckInItem>(sql,
                                        (obj, inventoryCheckIn, item, location, uom, mfgCountry, currency) =>
                                        {
                                            obj.InventoryCheckIn = inventoryCheckIn;
                                            obj.Item = item;
                                            obj.Location = location;
                                            obj.Unit = uom;
                                            obj.MfgCountry = mfgCountry;
                                            obj.Currency = currency;

                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return data;
    }

    public async Task<List<InventoryCheckInItem>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null,
        List<int>? locationIdList = null,
        string? barcode = null,
        string? brand = null,
        string? batchID = null,
        decimal? totalAmountFrom = null,
        decimal? totalAmountTo = null,
        DateTime? mfgDateFrom = null,
        DateTime? mfgDateTo = null,
        DateTime? expiryDateFrom = null,
        DateTime? expiryDateTo = null,
        List<string>? mfgCountryCodeList = null)
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

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("LOWER(t.ObjectNameKh) LIKE '%'+LOWER(@ObjectNameKh)+'%'");
            param.Add("@ObjectNameKh", objectNameKh, DbType.AnsiString);
        }

        if (locationIdList != null && locationIdList.Any())
        {
            if (locationIdList.Count == 1)
            {
                sbSql.Where("t.LocationId=@LocationId");
                param.Add("@LocationId", locationIdList[0]);
            }
            else
            {
                sbSql.Where("t.LocationId IN @LocationIdList");
                param.Add("@LocationIdList", locationIdList);
            }
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(brand))
        {
            sbSql.Where("UPPER(t.Brand) LIKE '%'+UPPER(@Brand)+'%'");
            param.Add("@Brand", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(batchID))
        {
            sbSql.Where("UPPER(t.BatchID) LIKE '%'+UPPER(@BatchID)+'%'");
            param.Add("@BatchID", batchID, DbType.AnsiString);
        }

        if (totalAmountFrom != null)
        {
            sbSql.Where("t.TotalAmount IS NOT NULL AND t.TotalAmount>=@TotalAmountFrom");
            param.Add("@TotalAmountFrom", totalAmountFrom);

            if (totalAmountTo != null)
            {
                sbSql.Where("t.TotalAmount<=@TotalAmountTo");
                param.Add("@TotalAmountTo", totalAmountTo);
            }
        }
        else if (totalAmountTo != null)
        {
            sbSql.Where("t.TotalAmount IS NOT NULL AND t.TotalAmount<=@TotalAmountTo");
            param.Add("@TotalAmountTo", totalAmountTo);
        }

        if (mfgDateFrom != null)
        {
            sbSql.Where("t.MfgDate IS NOT NULL AND t.MfgDate>=@MfgDateFrom");
            param.Add("@MfgDateFrom", mfgDateFrom.Value);

            if (mfgDateTo != null)
            {
                sbSql.Where("t.MfgDate<=@MfgDateTo");
                param.Add("@MfgDateTo", mfgDateTo.Value);
            }
        }
        else if (mfgDateTo != null)
        {
            sbSql.Where("t.MfgDate IS NOT NULL AND t.MfgDate<=@MfgDateTo");
            param.Add("@MfgDateTo", mfgDateTo.Value);
        }

        if (expiryDateFrom != null)
        {
            sbSql.Where("t.ExpiryDate IS NOT NULL AND t.ExpiryDate>=@ExpiryDateFrom");
            param.Add("@ExpiryDateFrom", expiryDateFrom.Value);

            if (expiryDateTo != null)
            {
                sbSql.Where("t.ExpiryDate<=@ExpiryDateTo");
                param.Add("@ExpiryDateTo", expiryDateTo.Value);
            }
        }
        else if (expiryDateTo != null)
        {
            sbSql.Where("t.ExpiryDate IS NOT NULL AND t.ExpiryDate<=@ExpiryDateTo");
            param.Add("@ExpiryDateTo", expiryDateTo.Value);
        }

        if (mfgCountryCodeList != null && mfgCountryCodeList.Any())
        {
            if (mfgCountryCodeList.Count == 1)
            {
                sbSql.Where("t.MfgCountryCode=@MfgCountryCode");
                param.Add("@MfgCountryCode", mfgCountryCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MfgCountryCode IN @MfgCountryCodeList");
                param.Add("@MfgCountryCodeList", mfgCountryCodeList);
            }
        }
        #endregion

        sbSql.LeftJoin($"{InventoryCheckIn.MsSqlTable} ici ON ici.Id=t.InventoryCheckInId");
        sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
        sbSql.LeftJoin($"{Location.MsSqlTable} l ON l.Id=t.LocationId");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} uom ON uom.IsDeleted=0 AND uom.ObjectCode=t.UnitCode");
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.MfgCountryCode");
        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");

        sbSql.OrderBy("t.CheckInDateTime DESC");
        sbSql.OrderBy("t.ObjectCode DESC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                    $"SELECT t.*, ici.*, i.*, l.*, uom.*, cty.*, curr.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<InventoryCheckInItem, InventoryCheckIn, Item, Location, UnitOfMeasure, Country, Currency, InventoryCheckInItem>(sql,
                                        (obj, inventoryCheckIn, item, location, uom, mfgCountry, currency) =>
                                        {
                                            obj.InventoryCheckIn = inventoryCheckIn;
                                            obj.Item = item;
                                            obj.Location = location;
                                            obj.Unit = uom;
                                            obj.MfgCountry = mfgCountry;
                                            obj.Currency = currency;

                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return data;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null,
        List<int>? locationIdList = null,
        string? barcode = null,
        string? brand = null,
        string? batchID = null,
        decimal? totalAmountFrom = null,
        decimal? totalAmountTo = null,
        DateTime? mfgDateFrom = null,
        DateTime? mfgDateTo = null,
        DateTime? expiryDateFrom = null,
        DateTime? expiryDateTo = null,
        List<string>? mfgCountryCodeList = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted = 0");

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

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("LOWER(t.ObjectNameKh) LIKE '%'+LOWER(@ObjectNameKh)+'%'");
            param.Add("@ObjectNameKh", objectNameKh, DbType.AnsiString);
        }

        if (locationIdList != null && locationIdList.Any())
        {
            if (locationIdList.Count == 1)
            {
                sbSql.Where("t.LocationId=@LocationId");
                param.Add("@LocationId", locationIdList[0]);
            }
            else
            {
                sbSql.Where("t.LocationId IN @LocationIdList");
                param.Add("@LocationIdList", locationIdList);
            }
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(brand))
        {
            sbSql.Where("UPPER(t.Brand) LIKE '%'+UPPER(@Brand)+'%'");
            param.Add("@Brand", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(batchID))
        {
            sbSql.Where("UPPER(t.BatchID) LIKE '%'+UPPER(@BatchID)+'%'");
            param.Add("@BatchID", batchID, DbType.AnsiString);
        }

        if (totalAmountFrom != null)
        {
            sbSql.Where("t.TotalAmount IS NOT NULL AND t.TotalAmount>=@TotalAmountFrom");
            param.Add("@TotalAmountFrom", totalAmountFrom);

            if (totalAmountTo != null)
            {
                sbSql.Where("t.TotalAmount<=@TotalAmountTo");
                param.Add("@TotalAmountTo", totalAmountTo);
            }
        }
        else if (totalAmountTo != null)
        {
            sbSql.Where("t.TotalAmount IS NOT NULL AND t.TotalAmount<=@TotalAmountTo");
            param.Add("@TotalAmountTo", totalAmountTo);
        }

        if (mfgDateFrom != null)
        {
            sbSql.Where("t.MfgDate IS NOT NULL AND t.MfgDate>=@MfgDateFrom");
            param.Add("@MfgDateFrom", mfgDateFrom.Value);

            if (mfgDateTo != null)
            {
                sbSql.Where("t.MfgDate<=@MfgDateTo");
                param.Add("@MfgDateTo", mfgDateTo.Value);
            }
        }
        else if (mfgDateTo != null)
        {
            sbSql.Where("t.MfgDate IS NOT NULL AND t.MfgDate<=@MfgDateTo");
            param.Add("@MfgDateTo", mfgDateTo.Value);
        }

        if (expiryDateFrom != null)
        {
            sbSql.Where("t.ExpiryDate IS NOT NULL AND t.ExpiryDate>=@ExpiryDateFrom");
            param.Add("@ExpiryDateFrom", expiryDateFrom.Value);

            if (expiryDateTo != null)
            {
                sbSql.Where("t.ExpiryDate<=@ExpiryDateTo");
                param.Add("@ExpiryDateTo", expiryDateTo.Value);
            }
        }
        else if (expiryDateTo != null)
        {
            sbSql.Where("t.ExpiryDate IS NOT NULL AND t.ExpiryDate<=@ExpiryDateTo");
            param.Add("@ExpiryDateTo", expiryDateTo.Value);
        }

        if (mfgCountryCodeList != null && mfgCountryCodeList.Any())
        {
            if (mfgCountryCodeList.Count == 1)
            {
                sbSql.Where("t.MfgCountryCode=@MfgCountryCode");
                param.Add("@MfgCountryCode", mfgCountryCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MfgCountryCode IN @MfgCountryCodeList");
                param.Add("@MfgCountryCodeList", mfgCountryCodeList);
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / pgSize);
        DataPagination pagination = new()
        {
            ObjectType = typeof(InventoryCheckIn).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}