using DataLayer.Models.Hobby;
using DataLayer.Models.HomeInventory;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.Hobby;

public interface IBoardgameRepos : IBaseRepos<Boardgame>
{
	Task<Boardgame?> GetFullAsync(int id);
	Task<int> InsertOrUpdateFullAsync(Boardgame obj);

	Task<List<Boardgame>> GetExpansionsAsync(int boardgameId);

	Task<List<Boardgame>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? barcode = null,
		string? isbn = null,
		int? releaseYearFrom = null,
		int? releaseYearTo = null,
		DateTime? purchasedDateFrom = null,
		DateTime? purchasedDateTo = null,
		decimal? purchasedPriceFrom = null,
		decimal? purchasedPriceTo = null,
		string? gamePublisher = null,
		string? gameDesignerName = null,
		int? numberOfPlayers = null,
		int? playDuration = null,
		List<string>? statusList = null,
		List<int>? merchantList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? barcode = null,
		string? isbn = null,
		int? releaseYearFrom = null,
		int? releaseYearTo = null,
		DateTime? purchasedDateFrom = null,
		DateTime? purchasedDateTo = null,
		decimal? purchasedPriceFrom = null,
		decimal? purchasedPriceTo = null,
		string? gamePublisher = null,
		string? gameDesignerName = null,
		int? numberOfPlayers = null,
		int? playDuration = null,
		List<string>? statusList = null,
		List<int>? merchantList = null);

	Task<List<DropdownSelectItem>> GetForDropdown1Async(int pgSize = 0, int pgNo = 0, string? searchText = null);

	Task<List<DropdownSelectItem>> GetExistingVersionAsync();
	Task<List<DropdownSelectItem>> GetExistingEditionListAsync();
}

public class BoardgameRepos(IDbContext dbContext) : BaseRepos<Boardgame>(dbContext, Boardgame.DatabaseObject), IBoardgameRepos
{
	public async Task<Boardgame?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();
        sbSql.LeftJoin($"{Merchant.MsSqlTable} m ON m.Id=t.MerchantId");
		sbSql.LeftJoin($"{Boardgame.MsSqlTable} mb ON mb.IsDeleted=0 AND mb.Id=t.MainBoardGameId");
		sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} su ON su.IsDeleted=0 AND su.ObjectCode=t.SizeUnitCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} pu ON pu.IsDeleted=0 AND pu.ObjectCode=t.PlayDurationUnitCode");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        var data = (await cn.QueryAsync<Boardgame, Merchant, Boardgame, UnitOfMeasure, UnitOfMeasure, Boardgame>(sql,
                                    (bg, m, mbg, su, pu) =>
                                    {
                                        bg.Merchant = m;
                                        bg.MainBoardGame = mbg;
                                        bg.SizeUnit = su;
                                        bg.DurationUnit = pu;

                                        return bg;
                                    }, new { Id=id }, splitOn: "Id")).FirstOrDefault();


        if (data != null)
        {
            SqlBuilder sbSqlContent = new();
            sbSqlContent.Where("bgci.IsDeleted=0");
            sbSqlContent.Where("bgci.BoardgameId=@BoardgameId");

            string sqlContent = sbSqlContent.AddTemplate($"SELECT * FROM {BoardgameContentItem.MsSqlTable} bgci /**where**/").RawSql;

            data!.ContentItems = (await cn.QueryAsync<BoardgameContentItem>(sqlContent, new { BoardgameId = data.Id })).AsList();
        }

        return data;
    }

    public async Task<int> InsertOrUpdateFullAsync(Boardgame obj)
    {
        using var cn = DbContext.DbCxn;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();
        using var tran = cn.BeginTransaction();

        try
        {
            int objId = -1;

            if (obj.Id == 0)
            {
                objId = await cn.InsertAsync(obj, tran);

				if (objId > 0)
				{
					foreach (BoardgameContentItem item in obj.ContentItems)
					{
						if (!item.IsDeleted)
						{
							item.CreatedDateTime = obj.CreatedDateTime;
							item.CreatedUser = obj.CreatedUser;
							item.ModifiedDateTime = obj.ModifiedDateTime;
							item.ModifiedUser = obj.ModifiedUser;

							int itemId = await cn.InsertAsync(item, tran);

							if (itemId <= 0)
								throw new Exception("Fail to insert BoardgameContentItem");
						}
					}
				}
				else
					throw new Exception("Fail to insert Boardgame");
			}
            else
            {
				bool isUpdated = await cn.UpdateAsync(obj, tran);

				if (isUpdated)
				{
					foreach (BoardgameContentItem item in obj.ContentItems)
					{
						if (item.IsDeleted && item.Id == 0)
							continue;

						if (item.Id == 0)
						{
							item.CreatedDateTime = obj.ModifiedDateTime;
							item.CreatedUser = obj.ModifiedUser;
							item.ModifiedDateTime = obj.ModifiedDateTime;
							item.ModifiedUser = obj.ModifiedUser;

							int itemId = await cn.InsertAsync(item, tran);

							if (itemId <= 0)
								throw new Exception("Fail to insert BoardgameContentItem");
						}
						else
						{
							item.ModifiedDateTime = obj.ModifiedDateTime;
							item.ModifiedUser = obj.ModifiedUser;
							bool isItemUpdated = await cn.UpdateAsync(item, tran);

							if (!isItemUpdated)
								throw new Exception("Fail to update BoardgameContentItem");
						}
					}

                    objId = obj.Id;
				}
				else
					throw new Exception("Fail to insert Boardgame");

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

    public async Task<List<Boardgame>> GetExpansionsAsync(int boardgameId)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.IsExpansion=1 AND t.MainBoardGameId=@MainBoardGameId";

		using var cn = DbContext.DbCxn;
        var dataList = (await cn.QueryAsync<Boardgame>(sql, new { MainBoardGameId = boardgameId })).AsList();

        return dataList;
	}

	public override async Task<KeyValuePair<int, IEnumerable<Boardgame>>> SearchNewAsync(
        int pgSize = 0, int pgNo = 0, string? searchText = null, 
        IEnumerable<SqlSortCond>? sortConds = null, 
        IEnumerable<SqlFilterCond>? filterConds = null, 
        List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (Regex.IsMatch(searchText, @"^[0-9]{5,}$"))
			{
				sbSql.Where("t.Barcode=@SearchText");
				param.Add("@SearchText", searchText);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText);
			}
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

		sbSql.LeftJoin($"{Merchant.MsSqlTable} m ON m.Id=t.MerchantId");
		sbSql.LeftJoin($"{Boardgame.MsSqlTable} mbg ON mbg.IsDeleted=0 AND mbg.Id=t.MainBoardGameId");
		sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} s ON s.IsDeleted=0 AND s.ObjectCode=t.SizeUnitCode");

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

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<Boardgame, Merchant, Boardgame, UnitOfMeasure, Boardgame>(sql,
										(b, m, mbg, su) => {
											b.Merchant = m;
											b.MainBoardGame = mbg;
											b.SizeUnit = su;
											return b;
										}, param: param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

    public override async Task<List<Boardgame>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        Regex regexNum = new(@"^[0-9]{1,}$");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
        {
            if (regexNum.IsMatch(searchText))
            {
                sbSql.Where("t.Barcode IS NOT NULL");
				sbSql.Where("t.Barcode=@SearchText");
                param.Add("@SearchText", searchText, DbType.AnsiString);
			}
            else
            {
				sbSql.Where("(UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%' OR UPPER(t.ObjectName) LIKE '%'+@SearchText+'%')");
				param.Add("@SearchText", searchText.ToUpper(), DbType.AnsiString);
			}
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{Merchant.MsSqlTable} m ON m.Id=t.MerchantId");
		sbSql.LeftJoin($"{Boardgame.MsSqlTable} mb ON mb.IsDeleted=0 AND mb.Id=t.MainBoardGameId");
		sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} su ON su.IsDeleted=0 AND su.ObjectCode=t.SizeUnitCode");

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
                $"SELECT t.*, m.*, mb.*, su.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Boardgame, Merchant, Boardgame, UnitOfMeasure, Boardgame>(sql,
                                    (bg, merchant, mbg, su) =>
                                    {
                                        bg.Merchant = merchant;
                                        bg.MainBoardGame = mbg;
                                        bg.SizeUnit = su;

                                        return bg;
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
            //Regex alphabets = new(@"^[a-zA-Z ]{1,}$");
            //Regex numbers = new(@"^[0-9\-]{1,}$");

            sbSql.Where("(UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%' OR UPPER(t.ObjectName) LIKE '%'+@SearchText+'%')");
            param.Add("@SearchText", searchText.ToUpper(), DbType.AnsiString);
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Boardgame).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<Boardgame>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? barcode = null,
        string? isbn = null,
        int? releaseYearFrom = null,
        int? releaseYearTo = null,
        DateTime? purchasedDateFrom = null,
        DateTime? purchasedDateTo = null,
        decimal? purchasedPriceFrom = null,
        decimal? purchasedPriceTo = null,
        string? gamePublisher = null,
        string? gameDesignerName = null,
        int? numberOfPlayers = null,
        int? playDuration = null,
        List<string>? statusList = null,
        List<int>? merchantList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(isbn))
        {
            sbSql.Where("t.Isbn LIKE '%'+@Isbn+'%'");
            param.Add("@Isbn", isbn, DbType.AnsiString);
        }

        if (releaseYearFrom.HasValue)
        {
            sbSql.Where("t.ReleaseYear IS NOT NULL");
            sbSql.Where("t.ReleaseYear>=@ReleaseYearFrom");
            param.Add("@ReleaseYearFrom", releaseYearFrom.Value);

            if (releaseYearTo.HasValue)
            {
                sbSql.Where("t.ReleaseYear<=@ReleaseYearTo");
                param.Add("@ReleaseYearTo", releaseYearTo.Value);
            }
        }
        else if (releaseYearTo.HasValue)
        {
            sbSql.Where("t.ReleaseYear IS NOT NULL");
            sbSql.Where("t.ReleaseYear<=@ReleaseYearTo");

            param.Add("@ReleaseYearTo", releaseYearTo.Value);
        }

        if (purchasedDateFrom.HasValue)
        {
            sbSql.Where("t.PurchasedDate IS NOT NULL");
            sbSql.Where("t.PurchasedDate>=@PurchasedDateFrom");
            param.Add("@PurchasedDateFrom", purchasedDateFrom.Value);

            if (purchasedDateTo.HasValue)
            {
                sbSql.Where("t.PurchasedDate<=@PurchasedDateTo");
                param.Add("@PurchasedDate", purchasedDateTo.Value);
            }
        }
        else if (purchasedDateTo.HasValue)
        {
            sbSql.Where("t.PurchasedDate IS NOT NULL");
            sbSql.Where("t.PurchasedDate<=@PurchasedDateTo");
            param.Add("@PurchasedDate", purchasedDateTo.Value);
        }

        if (purchasedPriceFrom.HasValue)
        {
            sbSql.Where("t.PurchasedPrice IS NOT NULL");
            sbSql.Where("t.PurchasedPrice>=@PurchasedPriceFrom");
            param.Add("@PurchasedPriceFrom", purchasedPriceFrom.Value);

            if (purchasedPriceTo.HasValue)
            {
                sbSql.Where("t.PurchasedPrice<=@PurchasedPriceTo");
                param.Add("@PurchasedPrice", purchasedPriceTo.Value);
            }
        }
        else if (purchasedPriceTo.HasValue)
        {
            sbSql.Where("t.PurchasedPrice IS NOT NULL");
            sbSql.Where("PurchasedPrice<=@PurchasedPriceTo");
            param.Add("@PurchasedPrice", purchasedPriceTo.Value);
        }

        if (!string.IsNullOrEmpty(gamePublisher))
        {
            sbSql.Where("LOWER(t.GamePublisher) LIKE '%'+LOWER(@GamePublisher)+'%'");
            param.Add("@GamePublisher", gamePublisher, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(gameDesignerName))
        {
            sbSql.Where("LOWER(t.GameDesignerName) LIKE '%'+LOWER(@GameDesignerName)+'%'");
            param.Add("@GameDesignerName", gameDesignerName, DbType.AnsiString);
        }

        if (numberOfPlayers.HasValue)
        {
            sbSql.Where("(t.MinPlayerNumber IS NULL OR t.MinPlayerNumber<=@NumberOfPlayers) AND (t.MaxPlayerNumber IS NULL OR t.MaxPlayerNumber>=@MaxPlayerNumber)");
            param.Add("@MaxPlayerNumber", numberOfPlayers.Value);
        }

        if (playDuration.HasValue)
        {
            sbSql.Where("(t.MinPlayDuration IS NULL OR t.MinPlayDuration<=@PlayDuration) AND (t.MaxPlayDuration IS NULL OR t.MaxPlayDuration>=@PlayDuration)");
            param.Add("@PlayDuration", playDuration.Value);
        }

        if (statusList != null && statusList.Any())
        {
            if (statusList.Count == 1)
            {
                sbSql.Where("t.[Status]=@Status");
                param.Add("@Status", statusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.[Status] IN @StatusList");
                param.Add("@StatusList", statusList);
            }
        }

        if (merchantList != null && merchantList.Any())
        {
            if (merchantList.Count == 1)
            {
                sbSql.Where("t.MerchantId=@MerchantId");
                param.Add("@MerchantId", merchantList[0]);
            }
            else
            {
                sbSql.Where("t.MerichantId IN @MerchantList");
                param.Add("@MerchantList", merchantList);
            }
        }
        #endregion

        sbSql.LeftJoin($"{Merchant.MsSqlTable} m ON m.Id=t.MerchantId");
		sbSql.LeftJoin($"{Boardgame.MsSqlTable} mb ON mbg.IsDeleted=0 AND mbg.Id=t.MainBoardGameId");
		sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} s ON s.IsDeleted=0 AND s.ObjectCode=t.SizeUnitCode");

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
                  $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.*, m.*, mb.*, s.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<Boardgame> result = (await cn.QueryAsync<Boardgame, Merchant, Boardgame, UnitOfMeasure, Boardgame>(sql,
                                        (b, m, mbg, su) => {
                                            b.Merchant = m;
                                            b.MainBoardGame = mbg;
                                            b.SizeUnit = su;
                                            return b;
                                        },param: param, splitOn: "Id")).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? barcode = null,
        string? isbn = null,
        int? releaseYearFrom = null,
        int? releaseYearTo = null,
        DateTime? purchasedDateFrom = null,
        DateTime? purchasedDateTo = null,
        decimal? purchasedPriceFrom = null,
        decimal? purchasedPriceTo = null,
        string? gamePublisher = null,
        string? gameDesignerName = null,
        int? numberOfPlayers = null,
        int? playDuration = null,
        List<string>? statusList = null,
        List<int>? merchantList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(isbn))
        {
            sbSql.Where("t.Isbn LIKE '%'+@Isbn+'%'");
            param.Add("@Isbn", isbn, DbType.AnsiString);
        }

        if (releaseYearFrom.HasValue)
        {
            sbSql.Where("t.ReleaseYear IS NOT NULL");
            sbSql.Where("t.ReleaseYear>=@ReleaseYearFrom");
            param.Add("@ReleaseYearFrom", releaseYearFrom.Value);

            if (releaseYearTo.HasValue)
            {
                sbSql.Where("t.ReleaseYear<=@ReleaseYearTo");
                param.Add("@ReleaseYearTo", releaseYearTo.Value);
            }
        }
        else if (releaseYearTo.HasValue)
        {
            sbSql.Where("t.ReleaseYear IS NOT NULL");
            sbSql.Where("t.ReleaseYear<=@ReleaseYearTo");

            param.Add("@ReleaseYearTo", releaseYearTo.Value);
        }

        if (purchasedDateFrom.HasValue)
        {
            sbSql.Where("t.PurchasedDate IS NOT NULL");
            sbSql.Where("t.PurchasedDate>=@PurchasedDateFrom");
            param.Add("@PurchasedDateFrom", purchasedDateFrom.Value);

            if (purchasedDateTo.HasValue)
            {
                sbSql.Where("t.PurchasedDate<=@PurchasedDateTo");
                param.Add("@PurchasedDate", purchasedDateTo.Value);
            }
        }
        else if (purchasedDateTo.HasValue)
        {
            sbSql.Where("t.PurchasedDate IS NOT NULL");
            sbSql.Where("t.PurchasedDate<=@PurchasedDateTo");
            param.Add("@PurchasedDate", purchasedDateTo.Value);
        }

        if (purchasedPriceFrom.HasValue)
        {
            sbSql.Where("t.PurchasedPrice IS NOT NULL");
            sbSql.Where("t.PurchasedPrice>=@PurchasedPriceFrom");
            param.Add("@PurchasedPriceFrom", purchasedPriceFrom.Value);

            if (purchasedPriceTo.HasValue)
            {
                sbSql.Where("t.PurchasedPrice<=@PurchasedPriceTo");
                param.Add("@PurchasedPrice", purchasedPriceTo.Value);
            }
        }
        else if (purchasedPriceTo.HasValue)
        {
            sbSql.Where("t.PurchasedPrice IS NOT NULL");
            sbSql.Where("PurchasedPrice<=@PurchasedPriceTo");
            param.Add("@PurchasedPrice", purchasedPriceTo.Value);
        }

        if (!string.IsNullOrEmpty(gamePublisher))
        {
            sbSql.Where("LOWER(t.GamePublisher) LIKE '%'+LOWER(@GamePublisher)+'%'");
            param.Add("@GamePublisher", gamePublisher, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(gameDesignerName))
        {
            sbSql.Where("LOWER(t.GameDesignerName) LIKE '%'+LOWER(@GameDesignerName)+'%'");
            param.Add("@GameDesignerName", gameDesignerName, DbType.AnsiString);
        }

        if (numberOfPlayers.HasValue)
        {
            sbSql.Where("(t.MinPlayerNumber IS NULL OR t.MinPlayerNumber<=@NumberOfPlayers) AND (t.MaxPlayerNumber IS NULL OR t.MaxPlayerNumber>=@MaxPlayerNumber)");
            param.Add("@MaxPlayerNumber", numberOfPlayers.Value);
        }

        if (playDuration.HasValue)
        {
            sbSql.Where("(t.MinPlayDuration IS NULL OR t.MinPlayDuration<=@PlayDuration) AND (t.MaxPlayDuration IS NULL OR t.MaxPlayDuration>=@PlayDuration)");
            param.Add("@PlayDuration", playDuration.Value);
        }

        if (statusList != null && statusList.Any())
        {
            if (statusList.Count == 1)
            {
                sbSql.Where("t.[Status]=@Status");
                param.Add("@Status", statusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.[Status] IN @StatusList");
                param.Add("@StatusList", statusList);
            }
        }

        if (merchantList != null && merchantList.Any())
        {
            if (merchantList.Count == 1)
            {
                sbSql.Where("t.MerchantId=@MerchantId");
                param.Add("@MerchantId", merchantList[0]);
            }
            else
            {
                sbSql.Where("t.MerichantId IN @MerchantList");
                param.Add("@MerchantList", merchantList);
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Boardgame).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdown1Async(int pgSize = 0, int pgNo = 0, string? searchText = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");

        sbSql.Select("t.Id");
        sbSql.Select("'Key'=t.ObjectCode");
		sbSql.Select("'Value'=t.ObjectName+(CASE WHEN t.ReleaseYear IS NOT NULL THEN ' ('+CAST(t.ReleaseYear AS VARCHAR(10))+')' ELSE '' END)");

		if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", searchText, DbType.AnsiString);
        }

        sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgSize == 0 && pgNo == 0)
        {
            sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**ordery**/").RawSql;
        }
        else if (pgSize > 0 && pgNo > 0)
        {
            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT /**select**/ FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }
        else
            return [];

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return dataList;
    }

    public async Task<List<DropdownSelectItem>> GetExistingVersionAsync()
    {
        string sql = $"SELECT 'Key'=UPPER(REPLACE(t.VersionDesc,' ','-')), 'Value'=t.VersionDesc FROM (SELECT DISTINCT VersionDesc FROM {Boardgame.MsSqlTable} WHERE IsDeleted=0 AND VersionDesc IS NOT NULL) t";

        using var cn = DbContext.DbCxn;
        var dataList = (await cn.QueryAsync<DropdownSelectItem>(sql)).AsList();
        return dataList;
    }

    public async Task<List<DropdownSelectItem>> GetExistingEditionListAsync()
    {
        string sql = $"SELECT 'Key'=UPPER(REPLACE(t.EditionDesc,' ','-')), 'Value'=t.EditionDesc FROM (SELECT DISTINCT EditionDesc FROM {Boardgame.MsSqlTable} WHERE IsDeleted=0 AND ISNULL(EditionDesc,'')<>'') t";
        using var cn = DbContext.DbCxn;
        var dataList = (await cn.QueryAsync<DropdownSelectItem>(sql)).AsList();
        return dataList;
    }
}