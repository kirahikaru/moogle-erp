using DataLayer.Models.LIB;
using System.Text.RegularExpressions;
using DataLayer.GlobalConstant;

namespace DataLayer.Repos.LIB;

public interface IUserBookRepos : IBaseRepos<UserBook>
{
	Task<List<UserBook>> GetByUserAndBookAsync(int userId, int bookId);
	Task<UserBook?> GetFullAsync(int id);

	Task<List<UserBook>> QuickSearchAsync(
		int userId,
		int pgSize = 0, int pgNo = 0,
		string? searchText = null);

	Task<DataPagination> GetQuickSearchPaginationAsync(
		int userId, int pgSize = 0, string? searchText = null);

	Task<List<UserBook>> SearchAsync(
		int userId,
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? title = null,
		string? seriesName = null,
		string? authorName = null,
		string? isbn = null,
		int? publishedYearFrom = null,
		int? publishedYearTo = null,
		string? publisherName = null,
		DateTime? purchaseDateFrom = null,
		DateTime? purchaseDateTo = null,
		decimal? purchasedPriceFrom = null,
		decimal? purchasedPriceTo = null,
		bool? isGift = null,
		bool? isRead = null,
		bool? isEbookAvailable = null,
		int? userRatingFrom = null,
		int? userRatingTo = null,
		List<string>? ownershipStatusList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int userId,
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? title = null,
		string? seriesName = null,
		string? authorName = null,
		string? isbn = null,
		int? publishedYearFrom = null,
		int? publishedYearTo = null,
		string? publisherName = null,
		DateTime? purchaseDateFrom = null,
		DateTime? purchaseDateTo = null,
		decimal? purchasedPriceFrom = null,
		decimal? purchasedPriceTo = null,
		bool? isGift = null,
		bool? isRead = null,
		bool? isEbookAvailable = null,
		int? userRatingFrom = null,
		int? userRatingTo = null,
		List<string>? ownershipStatusList = null);
}

public class UserBookRepos(IDbContext dbContext) : BaseRepos<UserBook>(dbContext, UserBook.DatabaseObject), IUserBookRepos
{
	public async Task<List<UserBook>> GetByUserAndBookAsync(int userId, int bookId)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.UserId=@UserId AND t.BookId=@BookId";
        DynamicParameters param = new();
        param.Add("@UserId", userId);
        param.Add("@BookId", bookId);

        using var cn = DbContext.DbCxn;

        List<UserBook> dataList = (await cn.QueryAsync<UserBook>(sql, param)).AsList();

        return dataList;
    }

    public async Task<UserBook?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();

        sbSql.LeftJoin($"{User.MsSqlTable} u ON u.Id=t.UserId");
        sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.Id=t.BookId");
        sbSql.LeftJoin($"{BookCategory.MsSqlTable} bc ON bc.Id=b.BookCategoryId");
        sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.Id=b.AuthorId");
        sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.Id=b.CoAuthorId");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        DynamicParameters param = new();
        param.Add("@Id", id);
        param.Add("@BookRole", BookRoles.AUTHOR, DbType.AnsiString);

        using var cn = DbContext.DbCxn;

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        UserBook? dataObj = (await cn.QueryAsync<UserBook?, User, Book, BookCategory, Person, Person, UserBook?>(sql, (obj, user, book, bookCtg, author, coAuthor) =>
        {
            if (obj != null)
            {
                obj.User = user;

                if (book != null)
                {
                    book.Category = bookCtg;
                    book.Author = author;
                    book.CoAuthor = coAuthor;
                    obj.Book = book;
                }
            }

            return obj;
        }, new { Id = id }, splitOn: "Id")).SingleOrDefault();

        if (dataObj != null && dataObj.Book != null)
        {
            string bookAddInfoQry = $"SELECT b.* FROM {BookBookGenre.MsSqlTable} a LEFT JOIN {BookGenre.MsSqlTable} b ON b.Id=BookGenreId WHERE a.BookId=@BookId; " +
                                    $"SELECT p.* FROM {BookPersonRoleMap.MsSqlTable} bprm LEFT JOIN {Person.MsSqlTable} p ON p.Id=bprm.PersonId WHERE bprm.BookRole=@BookRole AND bprm.BookId=@BookId;";

            DynamicParameters bookAddInfoQryParam = new();
            bookAddInfoQryParam.Add("@BookId", dataObj.BookId);
            bookAddInfoQryParam.Add("@BookRole", BookRoles.AUTHOR, DbType.AnsiString);

            using var multi = await cn.QueryMultipleAsync(bookAddInfoQry, bookAddInfoQryParam);

            dataObj.Book.Genres = (await multi.ReadAsync<BookGenre>()).AsList();
            dataObj.Book.Authors = (await multi.ReadAsync<Person>()).AsList();
        }

        return dataObj;
    }

    public async Task<List<UserBook>> QuickSearchAsync(
        int userId,
        int pgSize = 0, int pgNo = 0,
        string? searchText = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UserId=@UserId");
        param.Add("@UserId", userId);

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
        {
            Regex alphabets = new(@"^[a-zA-Z ]{1,}$");
            Regex numbers = new(@"^[0-9\-]{1,}$");

            if (!string.IsNullOrEmpty(searchText))
            {
                if (searchText.Length > 5 && searchText.StartsWith("isbn:", StringComparison.OrdinalIgnoreCase))
                {
                    sbSql.Where("(b.ISBN13 LIKE '%'+@SearchText+'%' OR b.ISBN10 LIKE '%'+@SearchText+'%')");
					param.Add("@SearchText", searchText.Replace("isbn:","", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
                }
                else if (numbers.IsMatch(searchText))
                {
					sbSql.Where("(b.ISBN13 LIKE '%'+@SearchText+'%' OR b.ISBN10 LIKE '%'+@SearchText+'%')");
					param.Add("@SearchText", searchText, DbType.AnsiString);
				}
                else
                {
                    sbSql.Where("UPPER(b.Title) LIKE '%'+UPPER(@Title)+'%'");
                    param.Add("@Title", searchText, DbType.AnsiString);
                }
            }
        }
		#endregion

		sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.IsDeleted=0 AND b.Id=t.BookId");
        sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.IsDeleted=0 AND auth.Id=b.AuthorId");
        sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.IsDeleted=0 AND coAuth.Id=b.CoAuthorId");
        sbSql.LeftJoin($"{BookCategory.MsSqlTable} bctg ON bctg.IsDeleted=0 AND bctg.Id=b.BookCategoryId");

        sbSql.OrderBy("b.Title ASC");

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
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t LEFT JOIN {Book.MsSqlTable} b ON b.IsDeleted=0 AND b.Id=t.BookId /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.*, b.*, auth.*, coAuth.*, bctg.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<UserBook> result = (await cn.QueryAsync<UserBook, Book, Person, Person, BookCategory, UserBook>(sql,
                                        (obj, book, auth, coAuth, bookCtg) =>
                                        {
                                            if (book != null)
                                            {
                                                book.Author = auth;
                                                book.CoAuthor = coAuth;
                                                book.Category = bookCtg;
                                                obj.Book = book;
                                            }

                                            return obj;
                                        }, param: param, splitOn: "Id")).AsList();

        return result;
    }

    public async Task<DataPagination> GetQuickSearchPaginationAsync(
        int userId, int pgSize = 0, string? searchText = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UserId=@UserId");
        param.Add("@UserId", userId);

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			Regex alphabets = new(@"^[a-zA-Z ]{1,}$");
			Regex numbers = new(@"^[0-9\-]{1,}$");

			if (!string.IsNullOrEmpty(searchText))
			{
				if (searchText.Length > 5 && searchText.StartsWith("isbn:", StringComparison.OrdinalIgnoreCase))
				{
					sbSql.Where("(b.ISBN13 LIKE '%'+@SearchText+'%' OR b.ISBN10 LIKE '%'+@SearchText+'%')");
					param.Add("@SearchText", searchText.Replace("isbn:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
				}
				else if (numbers.IsMatch(searchText))
				{
					sbSql.Where("(b.ISBN13 LIKE '%'+@SearchText+'%' OR b.ISBN10 LIKE '%'+@SearchText+'%')");
					param.Add("@SearchText", searchText, DbType.AnsiString);
				}
				else
				{
					sbSql.Where("UPPER(b.Title) LIKE '%'+UPPER(@Title)+'%'");
					param.Add("@Title", searchText, DbType.AnsiString);
				}
			}
		}
		#endregion

		sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.Id=t.BookId");

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(UserBook).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<UserBook>> SearchAsync(
        int userId,
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? title = null,
        string? seriesName = null,
        string? authorName = null,
        string? isbn = null,
        int? publishedYearFrom = null,
        int? publishedYearTo = null,
        string? publisherName = null,
        DateTime? purchaseDateFrom = null,
        DateTime? purchaseDateTo = null,
		decimal? purchasedPriceFrom = null,
		decimal? purchasedPriceTo = null,
		bool? isGift = null,
        bool? isRead = null,
        bool? isEbookAvailable = null,
        int? userRatingFrom = null,
        int? userRatingTo = null,
        List<string>? ownershipStatusList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UserId=@UserId");
        param.Add("@UserId", userId);

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("b.ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("b.ObjectName LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(title))
        {
            sbSql.Where("LOWER(b.[Title]) LIKE '%'+@Title+'%'");
            param.Add("@Title", title.ToLower());
        }

        if (!string.IsNullOrEmpty(seriesName))
        {
            sbSql.Where("LOWER(b.[SeriesName]) LIKE '%'+@SeriesName+'%'");
            param.Add("@SeriesName", seriesName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(isbn))
        {
            sbSql.Where("(UPPER(b.[ISBN13]) LIKE '%'+UPPER(@ISBN)+'%' OR UPPER(b.[ISBN10]) LIKE '%'+UPPER(@ISBN)+'%')");
            param.Add("@ISBN", isbn, DbType.AnsiString);
        }

        if (publishedYearFrom.HasValue)
        {
            sbSql.Where("b.PublishedYear IS NOT NULL");
            sbSql.Where("b.PublishedYear>=@PublishedYearFrom");
            param.Add("@PublishedYearFrom", publishedYearFrom.Value);

            if (publishedYearTo.HasValue)
            {
                sbSql.Where("b.PublishedYear<=@PublishedYearTo");
                param.Add("@PublishedYearTo", publishedYearTo.Value);
            }
        }
        else if (publishedYearTo.HasValue)
        {
            sbSql.Where("b.PublishedYear IS NOT NULL");
            sbSql.Where("b.PublishedYear<=@PublishedYearTo");
            param.Add("@PublishedYearTo", publishedYearTo.Value);
        }

        if (!string.IsNullOrEmpty(publisherName))
        {
            sbSql.Where("UPPER(b.PublisherName) LIKE '%'+UPPER(@PublisherName)+'%'");
            param.Add("@PublisherName", publisherName, DbType.AnsiString);
        }

        if (purchaseDateFrom != null)
        {
            sbSql.Where("t.PurchaseDate IS NOT NULL");
            sbSql.Where("t.PurchaseDate>=@PurchaseDateFrom");
            param.Add("@PurchaseDateFrom", purchaseDateFrom.Value);

            if (purchaseDateTo != null)
            {
                sbSql.Where("t.PurchaseDate<=@PurchaseDateTo");
                param.Add("@PurchaseDateTo", purchaseDateTo.Value);
            }
        }
        else if (purchaseDateTo != null)
        {
            sbSql.Where("t.PurchaseDate IS NOT NULL");
            sbSql.Where("t.PurchaseDate<=@PurchaseDateTo");
            param.Add("@PurchaseDateTo", purchaseDateTo.Value);
        }

		if (purchasedPriceFrom != null)
		{
			sbSql.Where("t.PurchsedPrice IS NOT NULL");
			sbSql.Where("t.PurchsedPrice>=@PurchsedPriceFrom");
			param.Add("@PurchsedPriceFrom", purchasedPriceFrom.Value);

			if (purchasedPriceTo != null)
			{
				sbSql.Where("t.PurchsedPrice<=@PurchsedPriceTo");
				param.Add("@PurchsedPriceTo", purchasedPriceTo.Value);
			}
		}
		else if (purchasedPriceTo != null)
		{
			sbSql.Where("t.PurchsedPrice IS NOT NULL");
			sbSql.Where("t.PurchsedPrice<=@PurchaseDateTo");
			param.Add("@PurchsedPriceTo", purchasedPriceTo.Value);
		}

		if (isGift != null)
        {
            sbSql.Where("t.IsGift=@IsGift");
            param.Add("@IsGift", isGift.Value);
        }

        if (isRead != null)
        {
            sbSql.Where("t.IsRead=@IsRead");
            param.Add("@IsRead", isRead.Value);
        }

        if (isEbookAvailable != null)
        {
            sbSql.Where("t.IsEBookAvailable=@IsEBookAvailable");
            param.Add("@IsEBookAvailable", isEbookAvailable.Value);
        }

        if (userRatingFrom != null)
        {
            sbSql.Where("t.Rating IS NOT NULL");
            sbSql.Where("t.Rating>=@UserRatingFrom");
            param.Add("@UserRatingFrom", userRatingFrom.Value);

            if (userRatingTo != null)
            {
                sbSql.Where("t.Rating<=@UserRatingTo");
                param.Add("@UserRatingTo", userRatingTo.Value);
            }
        }
        else if (userRatingTo != null)
        {
            sbSql.Where("t.Rating IS NOT NULL");
            sbSql.Where("t.Rating<=@UserRatingTo");
            param.Add("@UserRatingTo", userRatingTo.Value);
        }

		if (ownershipStatusList != null && ownershipStatusList.Count != 0)
        {
            if (ownershipStatusList.Count == 1)
            {
                sbSql.Where("t.OwnershipStatus=@OwnershipStatus");
                param.Add("@OwnershipStatus", ownershipStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.OwnershipStatus IN @OwnershipStatusList");
                param.Add("@OwnershipStatusList", ownershipStatusList);
            }
        }
        #endregion

        using var cn = DbContext.DbCxn;

        if (!string.IsNullOrEmpty(authorName))
        {
            string[] authorNameParts = authorName.Split(' ');
            string findBookQry;
            List<int> bookIdList = new();

            if (authorNameParts.Length == 2)
            {
                findBookQry = $"SELECT DISTINCT b.BookId FROM {BookPersonRoleMap.MsSqlTable} ba " +
                              $"LEFT JOIN {Person.MsSqlTable} auth ON auth.Id=b.PersonId " +
                              $"WHERE ba.IsDeleted=0 AND ba.BookRole=@BookRole AND " +
                              $"((LOWER(auth.GivenName) LIKE '%'+@NamePart1+'%' AND LOWER(auth.Surname) LIKE '%'+@NamePart2+'%') OR " +
                              $"(LOWER(auth.GivenName) LIKE '%'+@NamePart2+'%' AND LOWER(auth.Surname) LIKE '%'+@NamePart1+'%'))";

                var findBookQryParam = new
                {
                    NamePart1 = authorNameParts[0].Trim().ToLower(),
                    NamePart2 = authorNameParts[1].Trim().ToLower(),
                    BookRole = new DbString { Value = BookRoles.AUTHOR, IsAnsi = true }
                };

                bookIdList = (await cn.QueryAsync<int>(findBookQry, findBookQryParam)).AsList();
            }
            else
            {
                findBookQry = $"SELECT DISTINCT b.BookId FROM {BookPersonRoleMap.MsSqlTable} ba " +
                              $"LEFT JOIN {Person.MsSqlTable} auth ON auth.Id=b.PersonId " +
                              $"WHERE ba.IsDeleted=0 AND ba.BookRole=@BookRole AND LOWER(ISNULL(auth.GivenName,'')+' '+ISNULL(auth.MiddleName,'')+' '+ISNULL(auth.Surname,'')) LIKE '%'+@Name+'%'";

                var findBookQryParam = new { Name = authorName.Trim().ToLower(), BookRole = new DbString { Value = BookRoles.AUTHOR, IsAnsi = true } };
                bookIdList = (await cn.QueryAsync<int>(findBookQry, findBookQryParam)).AsList();
            }

            sbSql.Where("t.BookId IN @BookIdList");
            param.Add("@BookIdList", bookIdList);
        }

        sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.IsDeleted=0 AND b.Id=t.BookId");
        sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.IsDeleted=0 AND auth.Id=b.AuthorId");
        sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.IsDeleted=0 AND coAuth.Id=b.CoAuthorId");
        sbSql.LeftJoin($"{BookCategory.MsSqlTable} bctg ON bctg.IsDeleted=0 AND bctg.Id=b.BookCategoryId");

        sbSql.OrderBy("b.[Title] ASC");
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
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t LEFT JOIN {Book.MsSqlTable} b ON b.Id=t.BookId /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.*, b.*, auth.*, coAuth.*, bctg.* FROM {DbObject.MsSqlTable} t " +
                $"INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        List<UserBook> result = (await cn.QueryAsync<UserBook, Book, BookCategory, Person, Person, UserBook>(
                                sql, (obj, book, bookCategory, author, coAuthor) => 
                                    {
                                        if (book != null)
                                        {
                                            book.Category = bookCategory;
                                            book.Author = author;
                                            book.CoAuthor = coAuthor;
                                            obj.Book = book;
                                        }
                                        
                                        return obj;
                                    }, param, splitOn: "Id")).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int userId,
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? title = null,
        string? seriesName = null,
        string? authorName = null,
		string? isbn = null,
		int? publishedYearFrom = null,
        int? publishedYearTo = null,
        string? publisherName = null,
        DateTime? purchaseDateFrom = null,
        DateTime? purchaseDateTo = null,
		decimal? purchasedPriceFrom = null,
		decimal? purchasedPriceTo = null,
		bool? isGift = null,
        bool? isRead = null,
        bool? isEbookAvailable = null,
        int? userRatingFrom = null,
        int? userRatingTo = null,
        List<string>? ownershipStatusList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.UserId=@UserId");

		param.Add("@UserId", userId);

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
		{
			sbSql.Where("b.ObjectCode LIKE @ObjectCode+'%'");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(objectName))
		{
			sbSql.Where("b.ObjectName LIKE '%'+@ObjectName+'%'");
			param.Add("@ObjectName", objectName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(title))
		{
			sbSql.Where("LOWER(b.[Title]) LIKE '%'+@Title+'%'");
			param.Add("@Title", title.ToLower());
		}

		if (!string.IsNullOrEmpty(seriesName))
		{
			sbSql.Where("LOWER(b.[SeriesName]) LIKE '%'+@SeriesName+'%'");
			param.Add("@SeriesName", seriesName.ToLower(), DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(isbn))
		{
			sbSql.Where("(UPPER(b.[ISBN13]) LIKE '%'+UPPER(@ISBN)+'%' OR UPPER(b.[ISBN10]) LIKE '%'+UPPER(@ISBN)+'%')");
			param.Add("@ISBN", isbn, DbType.AnsiString);
		}

		if (publishedYearFrom.HasValue)
		{
			sbSql.Where("b.PublishedYear IS NOT NULL");
			sbSql.Where("b.PublishedYear>=@PublishedYearFrom");
			param.Add("@PublishedYearFrom", publishedYearFrom.Value);

			if (publishedYearTo.HasValue)
			{
				sbSql.Where("b.PublishedYear<=@PublishedYearTo");
				param.Add("@PublishedYearTo", publishedYearTo.Value);
			}
		}
		else if (publishedYearTo.HasValue)
		{
			sbSql.Where("b.PublishedYear IS NOT NULL");
			sbSql.Where("b.PublishedYear<=@PublishedYearTo");
			param.Add("@PublishedYearTo", publishedYearTo.Value);
		}

		if (!string.IsNullOrEmpty(publisherName))
		{
			sbSql.Where("UPPER(b.PublisherName) LIKE '%'+UPPER(@PublisherName)+'%'");
			param.Add("@PublisherName", publisherName, DbType.AnsiString);
		}

		if (purchaseDateFrom != null)
		{
			sbSql.Where("t.PurchaseDate IS NOT NULL");
			sbSql.Where("t.PurchaseDate>=@PurchaseDateFrom");
			param.Add("@PurchaseDateFrom", purchaseDateFrom.Value);

			if (purchaseDateTo != null)
			{
				sbSql.Where("t.PurchaseDate<=@PurchaseDateTo");
				param.Add("@PurchaseDateTo", purchaseDateTo.Value);
			}
		}
		else if (purchaseDateTo != null)
		{
			sbSql.Where("t.PurchaseDate IS NOT NULL");
			sbSql.Where("t.PurchaseDate<=@PurchaseDateTo");
			param.Add("@PurchaseDateTo", purchaseDateTo.Value);
		}

		if (purchasedPriceFrom != null)
		{
			sbSql.Where("t.PurchsedPrice IS NOT NULL");
			sbSql.Where("t.PurchsedPrice>=@PurchsedPriceFrom");
			param.Add("@PurchsedPriceFrom", purchasedPriceFrom.Value);

			if (purchasedPriceTo != null)
			{
				sbSql.Where("t.PurchsedPrice<=@PurchsedPriceTo");
				param.Add("@PurchsedPriceTo", purchasedPriceTo.Value);
			}
		}
		else if (purchasedPriceTo != null)
		{
			sbSql.Where("t.PurchsedPrice IS NOT NULL");
			sbSql.Where("t.PurchsedPrice<=@PurchaseDateTo");
			param.Add("@PurchsedPriceTo", purchasedPriceTo.Value);
		}

		if (isGift != null)
		{
			sbSql.Where("t.IsGift=@IsGift");
			param.Add("@IsGift", isGift.Value);
		}

		if (isRead != null)
		{
			sbSql.Where("t.IsRead=@IsRead");
			param.Add("@IsRead", isRead.Value);
		}

		if (isEbookAvailable != null)
		{
			sbSql.Where("t.IsEBookAvailable=@IsEBookAvailable");
			param.Add("@IsEBookAvailable", isEbookAvailable.Value);
		}

		if (userRatingFrom != null)
		{
			sbSql.Where("t.Rating IS NOT NULL");
			sbSql.Where("t.Rating>=@UserRatingFrom");
			param.Add("@UserRatingFrom", userRatingFrom.Value);

			if (userRatingTo != null)
			{
				sbSql.Where("t.Rating<=@UserRatingTo");
				param.Add("@UserRatingTo", userRatingTo.Value);
			}
		}
		else if (userRatingTo != null)
		{
			sbSql.Where("t.Rating IS NOT NULL");
			sbSql.Where("t.Rating<=@UserRatingTo");
			param.Add("@UserRatingTo", userRatingTo.Value);
		}

		if (ownershipStatusList != null && ownershipStatusList.Count != 0)
		{
			if (ownershipStatusList.Count == 1)
			{
				sbSql.Where("t.OwnershipStatus=@OwnershipStatus");
				param.Add("@OwnershipStatus", ownershipStatusList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.OwnershipStatus IN @OwnershipStatusList");
				param.Add("@OwnershipStatusList", ownershipStatusList);
			}
		}
		#endregion

		using var cn = DbContext.DbCxn;

        if (!string.IsNullOrEmpty(authorName))
        {
            string[] authorNameParts = authorName.Split(' ');
            string findBookQry;
            List<int> bookIdList = new();

            if (authorNameParts.Length == 2)
            {
                findBookQry = $"SELECT DISTINCT b.BookId FROM {BookPersonRoleMap.MsSqlTable} ba " +
                              $"LEFT JOIN {Person.MsSqlTable} auth ON auth.Id=b.PersonId " +
                              $"WHERE ba.IsDeleted=0 AND ba.BookRole=@BookRole AND " +
                              $"((LOWER(auth.GivenName) LIKE '%'+@NamePart1+'%' AND LOWER(auth.Surname) LIKE '%'+@NamePart2+'%') OR " +
                              $"(LOWER(auth.GivenName) LIKE '%'+@NamePart2+'%' AND LOWER(auth.Surname) LIKE '%'+@NamePart1+'%'))";

                var findBookQryParam = new
                {
                    NamePart1 = authorNameParts[0].Trim().ToLower(),
                    NamePart2 = authorNameParts[1].Trim().ToLower(),
                    BookRole = new DbString { Value = BookRoles.AUTHOR, IsAnsi = true }
                };
                bookIdList = (await cn.QueryAsync<int>(findBookQry, findBookQryParam)).AsList();
            }
            else
            {
                findBookQry = $"SELECT DISTINCT b.BookId FROM {BookPersonRoleMap.MsSqlTable} ba " +
                              $"LEFT JOIN {Person.MsSqlTable} auth ON auth.Id=b.PersonId " +
                              $"WHERE ba.IsDeleted=0 AND ba.BookRole=@BookRole AND LOWER(ISNULL(auth.GivenName,'')+' '+ISNULL(auth.MiddleName,'')+' '+ISNULL(auth.Surname,'')) LIKE '%'+@Name+'%'";

                var findBookQryParam = new
                {
                    Name = authorName.Trim().ToLower(),
                    BookRole = new DbString { Value = BookRoles.AUTHOR, IsAnsi = true }
                };

                bookIdList = (await cn.QueryAsync<int>(findBookQry, findBookQryParam)).AsList();
            }

            sbSql.Where("t.BookId IN @BookIdList");
            param.Add("@BookIdList", bookIdList);
        }

        sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.Id=t.BookId");

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(UserBook).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}