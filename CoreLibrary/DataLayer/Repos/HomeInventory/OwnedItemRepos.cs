using DataLayer.Models.HomeInventory;
using DataLayer.Models.SystemCore.NonPersistent;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.HomeInventory;

public interface IOwnedItemRepos : IBaseRepos<OwnedItem>
{
	Task<OwnedItem?> GetFullAsync(int id);
	Task<List<OwnedItem>> GetByCategoryAsync(int ownedItemCategoryId);
	Task<List<OwnedItem>> GetByMerchantAsync(string merchantCode);

	Task<List<OwnedItem>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameEn = null,
		string? nameKh = null,
		List<int>? ownedItemCategoryIdList = null,
		string? barcode = null,
		string? brand = null,
		string? modelNo = null,
		string? serialNumber = null,
		string? otherRefNum1 = null,
		string? otherRefNum2 = null,
		string? itemDescription = null,
		string? specification = null,
		List<string>? statusList = null,
		DateTime? purchasedDateFrom = null,
		DateTime? purchasedDateTo = null,
		decimal? purchasePriceFrom = null,
		decimal? purchasePriceTo = null,
		List<string>? merchantObjectCodeList = null,
		List<string>? manufactureCountryList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameEn = null,
		string? nameKh = null,
		List<int>? ownedItemCategoryIdList = null,
		string? barcode = null,
		string? brand = null,
		string? modelNo = null,
		string? serialNumber = null,
		string? otherRefNum1 = null,
		string? otherRefNum2 = null,
		string? itemDescription = null,
		string? specification = null,
		List<string>? statusList = null,
		DateTime? purchasedDateFrom = null,
		DateTime? purchasedDateTo = null,
		decimal? purchasePriceFrom = null,
		decimal? purchasePriceTo = null,
		List<string>? merchantObjectCodeList = null,
		List<string>? manufactureCountryList = null);
}

public class OwnedItemRepos(IConnectionFactory connectionFactory) : BaseRepos<OwnedItem>(connectionFactory, OwnedItem.DatabaseObject), IOwnedItemRepos
{
	public async Task<OwnedItem?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.LeftJoin($"{OwnedItemCategory.MsSqlTable} oic ON oic.Id=t.OwnedItemCategoryId");
        sbSql.LeftJoin($"{Merchant.MsSqlTable} m ON m.IsDeleted=0 AND m.ObjectCode=t.MerchantObjectCode");
        sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=t.ManufacturerCountryCode");

        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<OwnedItem, OwnedItemCategory, Merchant, Country, OwnedItem>(
                                    sql,(oi, oic, m, c) => {
                                        oi.Category = oic;
                                        oi.Merchant = m;
                                        oi.ManufacturedCountry = c;
                                        return oi;
                                    }, new { Id=id }, splitOn:"Id")).AsList();

        if (dataList != null && dataList.Count != 0)
        {
            dataList[0].AuditTrails = (await cn.QueryAsync<ObjectStateHistory>($"SELECT * FROM {ObjectStateHistory.MsSqlTable} WHERE IsDeleted=0 AND ObjectName=@ObjectName AND ObjectId=@ObjectId ORDER BY EffectiveDate DESC, CreatedDateTime DESC",
                    new { ObjectName = dataList[0].GetType().Name, ObjectId= dataList[0].Id })).AsList();

			return dataList[0];
		}
        else
            return null;
    }

    public async Task<List<OwnedItem>> GetByCategoryAsync(int ownedItemCategoryId)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.OwnedItemCategoryId IS NOT NULL");
        sbSql.Where("t.OwnedItemCategoryId=@OwnedItemCategoryId");

        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        var param = new { OwnedItemCategoryId = ownedItemCategoryId };

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<OwnedItem> dataList = (await cn.QueryAsync<OwnedItem>(sql, param)).AsList();

        return dataList;
    }

    public async Task<List<OwnedItem>> GetByMerchantAsync(string merchantCode)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.MerchantObjectCode IS NOT NULL");
        sbSql.Where("t.MerchantObjectCode=@MerchantObjectCode");

        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        var param = new { MerchantObjectCode = new DbString { Value = merchantCode, IsAnsi = true } };

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<OwnedItem> dataList = (await cn.QueryAsync<OwnedItem>(sql, param)).AsList();

        return dataList;
    }

    public override async Task<List<OwnedItem>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            Regex alphabets = new(@"^[a-zA-Z0-9 ,.-]{1,}$");
            Regex numbers = new(@"^[0-9\-]{1,}$");

            if (searchText.Length >= 5 && (searchText.StartsWith("ctg:") || searchText.StartsWith("cat:")))
            {
                sbSql.Where("LOWER(oic.ObjectName) LIKE '%'+LOWER(@CategoryName)+'%'");
                param.Add("@CategoryName", searchText.Replace("ctg:", "", StringComparison.OrdinalIgnoreCase).Replace("cat:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (searchText.Length >= 3 && searchText.StartsWith("id:"))
            {
                sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+LOWER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (numbers.IsMatch(searchText))
			{
				sbSql.Where("t.Barcode LIKE '%'+@SearchText+'%'");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
			else if (alphabets.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.NameEn) LIKE '%'+UPPER(@SearchText)+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.NameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText, DbType.String);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{OwnedItemCategory.MsSqlTable} oic ON oic.Id=t.OwnedItemCategoryId");
        sbSql.LeftJoin($"{Merchant.MsSqlTable} m ON m.IsDeleted=0 AND m.ObjectCode=t.MerchantObjectCode");
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.ManufacturerCountryCode");

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
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t LEFT JOIN {OwnedItemCategory.MsSqlTable} oic ON oic.Id=t.OwnedItemCategoryId /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<OwnedItem> result = (await cn.QueryAsync<OwnedItem, OwnedItemCategory, Merchant, Country, OwnedItem>(sql,
                                        (oi, oic, m, cty) =>
                                        {
                                            oi.Category = oic;
                                            oi.Merchant = m;
                                            oi.ManufacturedCountry = cty;
                                            return oi;
                                        }, param, splitOn: "Id")).OrderBy(x => x.ObjectName).AsList();

        return result;
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
            Regex alphabets = new(@"^[a-zA-Z0-9 ,.-]{1,}$");
            Regex numbers = new(@"^[0-9\-]{1,}$");

            if (searchText.Length >= 5 && (searchText.StartsWith("ctg:") || searchText.StartsWith("cat:")))
            {
                sbSql.Where("LOWER(oic.ObjectName) LIKE '%'+LOWER(@CategoryName)+'%'");
                param.Add("@CategoryName", searchText.Replace("ctg:", "", StringComparison.OrdinalIgnoreCase).Replace("cat:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (numbers.IsMatch(searchText))
            {
                sbSql.Where("t.Barcode LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else if (alphabets.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.NameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText, DbType.String);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t LEFT JOIN {OwnedItemCategory.MsSqlTable} oic ON oic.Id=t.OwnedItemCategoryId /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(OwnedItem).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public override async Task<KeyValuePair<int, IEnumerable<OwnedItem>>> SearchNewAsync(
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
            sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
            param.Add("@SearchText", searchText);
        }

        if (excludeIdList != null && excludeIdList.Count != 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (filterConds != null && filterConds.Any())
        {
            foreach (SqlFilterCond filterCond in filterConds)
            {
                
            }
		}

		#endregion

		sbSql.LeftJoin($"{OwnedItemCategory.MsSqlTable} oic ON oic.Id=t.OwnedItemCategoryId");
		sbSql.LeftJoin($"{Merchant.MsSqlTable} m ON m.IsDeleted=0 AND m.ObjectCode=t.MerchantObjectCode");
		sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.ManufacturerCountryCode");

        if (sortConds is null || !sortConds.Any())
        {
			foreach (string order in GetSearchOrderbBy())
			{
				sbSql.OrderBy(order);
			}
		}
        else
        {
            foreach (SqlSortCond sortCond in sortConds)
            {
				sbSql.OrderBy(sortCond.GetSortCommand("t"));
			}
		}

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

		var dataList = await cn.QueryAsync<OwnedItem, OwnedItemCategory, Merchant, Country, OwnedItem>(sql,
										(oi, oic, m, cty) =>
										{
											oi.Category = oic;
											oi.Merchant = m;
											oi.ManufacturedCountry = cty;
											return oi;
										}, param, splitOn: "Id");

        string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
        return new(dataCount, dataList);
	}

    public async Task<List<OwnedItem>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameEn = null,
        string? nameKh = null,
        List<int>? ownedItemCategoryIdList = null,
        string? barcode = null,
        string? brand = null,
        string? modelNo = null,
        string? serialNumber = null,
        string? otherRefNum1 = null,
        string? otherRefNum2 = null,
        string? itemDescription = null,
        string? specification = null,
        List<string>? statusList = null,
        DateTime? purchasedDateFrom = null,
        DateTime? purchasedDateTo = null,
        decimal? purchasePriceFrom = null,
        decimal? purchasePriceTo = null,
        List<string>? merchantObjectCodeList = null,
        List<string>? manufactureCountryList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("LOWER(t.NameEn) LIKE '%'+@NameEn+'%'");
            param.Add("@NameEn", nameEn.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("t.NameKh LIKE '%'+@NameKh+'%'");
            param.Add("@NameKh", nameKh, DbType.String);
        }

        if (ownedItemCategoryIdList != null && ownedItemCategoryIdList.Any())
        {
            if (ownedItemCategoryIdList.Count == 1)
            {
                sbSql.Where("t.OwnedItemCategoryId IS NOT NULL AND t.OwnedItemCategoryId = @OwnedItemCategoryId");
                param.Add("@OwnedItemCategoryId", ownedItemCategoryIdList[0]);
            }
            else
            {
                sbSql.Where("t.OwnedItemCategoryId IN @OwnedItemCategoryIdList");
                param.Add("@OwnedItemCategoryIdList", ownedItemCategoryIdList);
            }
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(brand))
        {
            sbSql.Where("UPPER(t.Brand) LIKE '%'+@Brand+'%'");
            param.Add("@Brand", brand.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(modelNo))
        {
            sbSql.Where("UPPER(t.ModelNo) LIKE '%'+@ModelNo+'%'");
            param.Add("@ModelNo", modelNo.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(serialNumber))
        {
            sbSql.Where("UPPER(t.SerialNumber) LIKE '%'+@SerialNumber+'%'");
            param.Add("@SerialNumber", serialNumber.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(otherRefNum1))
        {
            sbSql.Where("UPPER(t.OtherRefNum1) LIKE '%'+@OtherRefNum1+'%'");
            param.Add("@OtherRefNum1", otherRefNum1.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(otherRefNum2))
        {
            sbSql.Where("UPPER(t.OtherRefNum2) LIKE '%'+@OtherRefNum2+'%'");
            param.Add("@OtherRefNum2", otherRefNum2.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(itemDescription))
        {
            sbSql.Where("UPPER(t.ItemDescription) LIKE '%'+@ItemDescription+'%'");
            param.Add("@ItemDescription", itemDescription.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(specification))
        {
            sbSql.Where("UPPER(t.Specification) LIKE '%'+@Specification+'%'");
            param.Add("@Specification", specification.ToUpper(), DbType.AnsiString);
        }

        if (purchasedDateFrom.HasValue)
        {
            sbSql.Where("t.PurchasedDate IS NOT NULL");
            sbSql.Where("t.PurchasedDate>=@PurchasedDateFrom");
            param.Add("@PurchasedDateFrom", purchasedDateFrom.Value);

            if (purchasedDateTo.HasValue)
            {
                sbSql.Where("t.PurchaseDate<=@PurchaseDateTo");
                param.Add("@PurchaseDateTo", purchasedDateTo.Value);
            }
        }
        else if (purchasedDateTo.HasValue)
        {
            sbSql.Where("t.PurchasedDate IS NOT NULL");
            sbSql.Where("t.PurchaseDate<=@PurchaseDateTo");
            param.Add("@PurchasedDateTo", purchasedDateTo.Value);
        }

        if (purchasePriceFrom.HasValue)
        {
            sbSql.Where("t.PurchasedPrice IS NOT NULL");
            sbSql.Where("t.PurchasedPrice>=@PurchasedPriceFrom");
            param.Add("@PurchasedPriceFrom", purchasePriceFrom.Value);

            if (purchasePriceTo.HasValue)
            {
                sbSql.Where("t.PurchasedPrice<=@PurchasedPriceTo");
                param.Add("@PurchasedPriceTo", purchasePriceTo.Value);
            }
        }
        else if (purchasePriceTo.HasValue)
        {
            sbSql.Where("t.PurchasedPrice IS NOT NULL");
            sbSql.Where("t.PurchasedPrice<=@PurchasedPriceTo");
            param.Add("@PurchasedPriceTo", purchasePriceTo.Value);
        }

        if (merchantObjectCodeList != null && merchantObjectCodeList.Any())
        {
            if (merchantObjectCodeList.Count == 1)
            {
                sbSql.Where("t.MerchantObjectCode IS NOT NULL");
                sbSql.Where("t.MerchantObjectCode = @MerchantObjectCode");
                param.Add("@MerchantObjectCode", merchantObjectCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MerchantObjectCode IN @MerchantObjectCodeList");
                param.Add("@MerchantObjectCodeList", merchantObjectCodeList);
            }
        }

        if (manufactureCountryList != null && manufactureCountryList.Any())
        {
            if (manufactureCountryList.Count == 1)
            {
                sbSql.Where("t.ManufacturerCountryCode IS NOT NULL");
                sbSql.Where("t.ManufacturerCountryCode = @ManufacturerCountryCode");
                param.Add("@ManufacturerCountryCode", manufactureCountryList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ManufacturerCountryCode IN @ManufacturerCountryCodeList");
                param.Add("@ManufacturerCountryCodeList", manufactureCountryList);
            }
        }
        #endregion

        sbSql.LeftJoin($"{OwnedItemCategory.MsSqlTable} oic ON oic.Id=t.OwnedItemCategoryId");
        sbSql.LeftJoin($"{Merchant.MsSqlTable} m ON m.IsDeleted=0 AND m.ObjectCode=t.MerchantObjectCode");
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.ManufacturerCountryCode");

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
                $"SELECT t.*, oic.*, m.*, cty.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<OwnedItem> result = (await cn.QueryAsync<OwnedItem, OwnedItemCategory, Merchant, Country, OwnedItem>(sql, 
                                        (oi, oic, m, cty) => 
                                        {
                                            oi.Category = oic;
                                            oi.Merchant = m;
                                            oi.ManufacturedCountry = cty;
                                            return oi;
                                        }, param, splitOn: "Id")).OrderBy(x => x.ObjectName).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameEn = null,
        string? nameKh = null,
        List<int>? ownedItemCategoryIdList = null,
        string? barcode = null,
        string? brand = null,
        string? modelNo = null,
        string? serialNumber = null,
        string? otherRefNum1 = null,
        string? otherRefNum2 = null,
        string? itemDescription = null,
        string? specification = null,
        List<string>? statusList = null,
        DateTime? purchasedDateFrom = null,
        DateTime? purchasedDateTo = null,
        decimal? purchasePriceFrom = null,
        decimal? purchasePriceTo = null,
        List<string>? merchantObjectCodeList = null,
        List<string>? manufactureCountryList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("LOWER(t.NameEn) LIKE '%'+@NameEn+'%'");
            param.Add("@NameEn", nameEn.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("t.NameKh LIKE '%'+@NameKh+'%'");
            param.Add("@NameKh", nameKh, DbType.String);
        }

        if (ownedItemCategoryIdList != null && ownedItemCategoryIdList.Any())
        {
            if (ownedItemCategoryIdList.Count == 1)
            {
                sbSql.Where("t.OwnedItemCategoryId IS NOT NULL AND t.OwnedItemCategoryId = @OwnedItemCategoryId");
                param.Add("@OwnedItemCategoryId", ownedItemCategoryIdList[0]);
            }
            else
            {
                sbSql.Where("t.OwnedItemCategoryId IN @OwnedItemCategoryIdList");
                param.Add("@OwnedItemCategoryIdList", ownedItemCategoryIdList);
            }
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(brand))
        {
            sbSql.Where("UPPER(t.Brand) LIKE '%'+@Brand+'%'");
            param.Add("@Brand", brand.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(modelNo))
        {
            sbSql.Where("UPPER(t.ModelNo) LIKE '%'+@ModelNo+'%'");
            param.Add("@ModelNo", modelNo.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(serialNumber))
        {
            sbSql.Where("UPPER(t.SerialNumber) LIKE '%'+@SerialNumber+'%'");
            param.Add("@SerialNumber", serialNumber.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(otherRefNum1))
        {
            sbSql.Where("UPPER(t.OtherRefNum1) LIKE '%'+@OtherRefNum1+'%'");
            param.Add("@OtherRefNum1", otherRefNum1.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(otherRefNum2))
        {
            sbSql.Where("UPPER(t.OtherRefNum2) LIKE '%'+@OtherRefNum2+'%'");
            param.Add("@OtherRefNum2", otherRefNum2.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(itemDescription))
        {
            sbSql.Where("UPPER(t.ItemDescription) LIKE '%'+@ItemDescription+'%'");
            param.Add("@ItemDescription", itemDescription.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(specification))
        {
            sbSql.Where("UPPER(t.Specification) LIKE '%'+@Specification+'%'");
            param.Add("@Specification", specification.ToUpper(), DbType.AnsiString);
        }

        if (purchasedDateFrom.HasValue)
        {
            sbSql.Where("t.PurchasedDate IS NOT NULL");
            sbSql.Where("t.PurchasedDate>=@PurchasedDateFrom");
            param.Add("@PurchasedDateFrom", purchasedDateFrom.Value);

            if (purchasedDateTo.HasValue)
            {
                sbSql.Where("t.PurchaseDate<=@PurchaseDateTo");
                param.Add("@PurchaseDateTo", purchasedDateTo.Value);
            }
        }
        else if (purchasedDateTo.HasValue)
        {
            sbSql.Where("t.PurchasedDate IS NOT NULL");
            sbSql.Where("t.PurchaseDate<=@PurchaseDateTo");
            param.Add("@PurchasedDateTo", purchasedDateTo.Value);
        }

        if (purchasePriceFrom.HasValue)
        {
            sbSql.Where("t.PurchasedPrice IS NOT NULL");
            sbSql.Where("t.PurchasedPrice>=@PurchasedPriceFrom");
            param.Add("@PurchasedPriceFrom", purchasePriceFrom.Value);

            if (purchasePriceTo.HasValue)
            {
                sbSql.Where("t.PurchasedPrice<=@PurchasedPriceTo");
                param.Add("@PurchasedPriceTo", purchasePriceTo.Value);
            }
        }
        else if (purchasePriceTo.HasValue)
        {
            sbSql.Where("t.PurchasedPrice IS NOT NULL");
            sbSql.Where("t.PurchasedPrice<=@PurchasedPriceTo");
            param.Add("@PurchasedPriceTo", purchasePriceTo.Value);
        }

        if (merchantObjectCodeList != null && merchantObjectCodeList.Any())
        {
            if (merchantObjectCodeList.Count == 1)
            {
                sbSql.Where("t.MerchantObjectCode IS NOT NULL");
                sbSql.Where("t.MerchantObjectCode = @MerchantObjectCode");
                param.Add("@MerchantObjectCode", merchantObjectCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MerchantObjectCode IN @MerchantObjectCodeList");
                param.Add("@MerchantObjectCodeList", merchantObjectCodeList);
            }
        }

        if (manufactureCountryList != null && manufactureCountryList.Any())
        {
            if (manufactureCountryList.Count == 1)
            {
                sbSql.Where("t.ManufacturerCountryCode IS NOT NULL");
                sbSql.Where("t.ManufacturerCountryCode = @ManufacturerCountryCode");
                param.Add("@ManufacturerCountryCode", manufactureCountryList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ManufacturerCountryCode IN @ManufacturerCountryCodeList");
                param.Add("@ManufacturerCountryCodeList", manufactureCountryList);
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(OwnedItemCategory).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}