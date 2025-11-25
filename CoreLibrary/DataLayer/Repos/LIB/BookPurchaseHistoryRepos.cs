using DataLayer.Models.LIB;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.LIB;

public interface IBookPurchaseHistoryRepos : IBaseRepos<BookPurchaseHistory>
{
	Task<List<BookPurchaseHistory>> GetByUserAsync(int userId);

	Task<List<BookPurchaseHistory>> GetByBookAsync(int bookId);
	Task<List<BookPurchaseHistory>> GetByUserBookAsync(int userBookId);

	Task<List<BookPurchaseHistory>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		List<int>? userIdList = null,
		DateTime? purchasedDateFrom = null,
		DateTime? purchasedDateTo = null,
		decimal? unitPriceFrom = null,
		decimal? unitPriceTo = null,
		string? bookTitle = null,
		string? authorName = null,
		List<int>? categoryList = null,
		List<string>? bookFormatList = null,
		bool? isEBook = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		List<int>? userIdList = null,
		DateTime? purchasedDateFrom = null,
		DateTime? purchasedDateTo = null,
		decimal? unitPriceFrom = null,
		decimal? unitPriceTo = null,
		string? bookTitle = null,
		string? authorName = null,
		List<int>? categoryList = null,
		List<string>? bookFormatList = null,
		bool? isEBook = null);
}

public class BookPurchaseHistoryRepos(IDbContext dbContext) : BaseRepos<BookPurchaseHistory>(dbContext, BookPurchaseHistory.DatabaseObject), IBookPurchaseHistoryRepos
{
	public async Task<List<BookPurchaseHistory>> GetByUserAsync(int userId)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("us.UserId=@UserId");

        param.Add("@UserId", userId);

		sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.Id=t.BookId");
		sbSql.LeftJoin($"{BookCategory.MsSqlTable} bc ON bc.Id=b.BookCategoryId");
		sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.Id=b.AuthorId");
		sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.Id=b.CoAuthorId");
		sbSql.LeftJoin($"{UserBook.MsSqlTable} ub ON ub.Id=t.UserBookId");

		using var cn = DbContext.DbCxn;

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		var dataList = (await cn.QueryAsync<BookPurchaseHistory, Book, BookCategory, Person, Person, UserBook, BookPurchaseHistory>(
							sql, (bph, book, category, author, coAuthor, userBook) =>
							{
								if (book != null)
								{
									book.Category = category;
									book.Author = author;
									book.CoAuthor = coAuthor;
									bph.Book = book;
                                    bph.UserBook = userBook;
								}

								return bph;
							}, param, splitOn: "Id")).AsList();

        return dataList;
	}

	public async Task<List<BookPurchaseHistory>> GetByBookAsync(int bookId)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.BookId=@BookId");

		param.Add("@BookId", bookId);

		sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.Id=t.BookId");
		sbSql.LeftJoin($"{BookCategory.MsSqlTable} bc ON bc.Id=b.BookCategoryId");
		sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.Id=b.AuthorId");
		sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.Id=b.CoAuthorId");
		sbSql.LeftJoin($"{UserBook.MsSqlTable} ub ON ub.Id=t.UserBookId");

		using var cn = DbContext.DbCxn;

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		var dataList = (await cn.QueryAsync<BookPurchaseHistory, Book, BookCategory, Person, Person, UserBook, BookPurchaseHistory>(
							sql, (bph, book, category, author, coAuthor, userBook) =>
							{
								if (book != null)
								{
									book.Category = category;
									book.Author = author;
									book.CoAuthor = coAuthor;
									bph.Book = book;
								}

								bph.UserBook = userBook;

								return bph;
							}, param, splitOn: "Id")).AsList();

		return dataList;
	}

	public async Task<List<BookPurchaseHistory>> GetByUserBookAsync(int userBookId)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.UserBookId=@UserBookId");

		param.Add("@UserBookId", userBookId);

		sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.Id=t.BookId");
		sbSql.LeftJoin($"{BookCategory.MsSqlTable} bc ON bc.Id=b.BookCategoryId");
		sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.Id=b.AuthorId");
		sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.Id=b.CoAuthorId");
		sbSql.LeftJoin($"{UserBook.MsSqlTable} ub ON ub.Id=t.UserBookId");

		using var cn = DbContext.DbCxn;

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		var dataList = (await cn.QueryAsync<BookPurchaseHistory, Book, BookCategory, Person, Person, UserBook, BookPurchaseHistory>(
							sql, (bph, book, category, author, coAuthor, userBook) =>
							{
								if (book != null)
								{
									book.Category = category;
									book.Author = author;
									book.CoAuthor = coAuthor;
									bph.Book = book;
								}

								bph.UserBook = userBook;

								return bph;
							}, param, splitOn: "Id")).AsList();

		return dataList;
	}

	public override async Task<List<BookPurchaseHistory>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("isbn:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("b.ISBN13 LIKE '%'+@Isbn13+'%'");
                param.Add("@Isbn13", searchText.Replace("isbn:", "", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                sbSql.Where("b.Title LIKE '%'+@Title+'%'");
                param.Add("@Title", searchText.Replace("title:", "", StringComparison.OrdinalIgnoreCase));
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.Id=t.BookId");
        sbSql.LeftJoin($"{BookCategory.MsSqlTable} bc ON bc.Id=b.BookCategoryId");
        sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.Id=b.AuthorId");
        sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.Id=b.CoAuthorId");
        sbSql.LeftJoin($"{UserBook.MsSqlTable} ub ON ub.Id=t.UserBookId");

        sbSql.OrderBy("t.PurchasedDate DESC");
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
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.*, b.*, bc.*, auth.*, coAuth.*, ub.* FROM {DbObject.MsSqlTable} t " +
                $"INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<BookPurchaseHistory, Book, BookCategory, Person, Person, UserBook, BookPurchaseHistory>(
											sql, (bph, book, category, author, coAuthor, userBook) =>
											{
												if (book != null)
												{
													book.Category = category;
													book.Author = author;
													book.CoAuthor = coAuthor;
													bph.Book = book;
												}

												bph.UserBook = userBook;

												return bph;
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
			if (searchText.StartsWith("isbn:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("b.ISBN13 LIKE '%'+@Isbn13+'%'");
				param.Add("@Isbn13", searchText.Replace("isbn:", "", StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				sbSql.Where("b.Title LIKE '%'+@Title+'%'");
				param.Add("@Title", searchText.Replace("title:", "", StringComparison.OrdinalIgnoreCase));
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.Id=t.BookId");
        sbSql.LeftJoin($"{BookCategory.MsSqlTable} bc ON bc.Id=b.BookCategoryId");
        sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.Id=b.AuthorId");
        sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.Id=b.CoAuthorId");
		sbSql.LeftJoin($"{UserBook.MsSqlTable} ub ON ub.Id=t.UserBookId");

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(BookPurchaseHistory).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<BookPurchaseHistory>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
		List<int>? userIdList = null,
		DateTime? purchasedDateFrom = null,
        DateTime? purchasedDateTo = null,
        decimal? unitPriceFrom = null,
        decimal? unitPriceTo = null,
        string? bookTitle = null,
        string? authorName = null,
        List<int>? categoryList = null,
        List<string>? bookFormatList = null,
        bool? isEBook = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("(t.ObjectCode LIKE '%'+@ObjectCode+'%' OR b.ISBN13 LIKE '%'+@ObjectCode+'%' OR b.ISBN10 LIKE '%'+@ObjectCode+'%')");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (userIdList is not null && userIdList.Count != 0)
        {
            if (userIdList.Count == 1)
            {
                sbSql.Where("ub.UserId=@UserId");
                param.Add("@UserId", userIdList[0]);
            }
            else
            {
				sbSql.Where("ub.UserId IN @UserIdList");
				param.Add("@UserIdList", userIdList);
			}
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
            sbSql.Where("t.PurchasedDate<=@PurchasedDateTo");
            param.Add("@PurchasedDateTo", purchasedDateTo.Value);
        }

        if (unitPriceFrom.HasValue)
        {
            sbSql.Where("t.UnitPrice IS NOT NULL");
            sbSql.Where("t.UnitPrice>=@UnitPriceFrom");
            param.Add("@UnitPriceFrom", unitPriceFrom.Value);

            if (unitPriceTo.HasValue)
            {
                sbSql.Where("t.UnitPrice<=@UnitPriceTo");
                param.Add("@UnitPriceTo", unitPriceTo.Value);
            }
        }
        else if (unitPriceTo.HasValue)
        {
            sbSql.Where("t.UnitPrice IS NOT NULL");
            sbSql.Where("t.UnitPrice<=@UnitPriceTo");
            param.Add("@UnitPriceTo", unitPriceTo.Value);
        }

        if (!string.IsNullOrEmpty(bookTitle))
        {
            sbSql.Where("UPPER(b.[Title]) LIKE '%'+UPPER(@BookTitle)+'%'");
            param.Add("@BookTitle", bookTitle, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(authorName))
        {
            Regex engName = new(@"^[a-zA-Z., ]{1,}$");

            if (engName.IsMatch(authorName))
            {
                sbSql.Where("((auth.ObjectName Is NOT NULL AND LOWER(ISNULL(auth.GivenName,'') + ' ' + ISNULL(auth.Surname,'')) LIKE '%'+LOWER(@AuthorName)+'%') OR " +
                    "(coAuth.ObjectName Is NOT NULL AND LOWER(ISNULL(coAuth.GivenName,'') + ' ' + ISNULL(coAuth.Surname,'')) LIKE '%'+LOWER(@AuthorName)+'%'))");
                param.Add("@AuthorName", authorName, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("((auth.ObjectName Is NOT NULL AND ISNULL(auth.SurnameKh,'') + ' ' + ISNULL(auth.GivenNameKh,'') LIKE '%'+@AuthorName+'%') OR " +
                    "(coAuth.ObjectName Is NOT NULL AND ISNULL(coAuth.SurnameKh,'') + ' ' + ISNULL(coAuth.GivenNameKh,'') LIKE '%'+@AuthorName+'%'))");
                param.Add("@AuthorName", authorName);
            }
        }

		if (categoryList != null && categoryList.Count != 0)
        {
            if (categoryList.Count == 1)
            {
                sbSql.Where("b.BookCategoryId=@BookCategoryId");
                param.Add("@BookCategoryId", categoryList[0]);
            }
            else
            {
                sbSql.Where("b.BookCategoryId IN @BookCategoryIdList");
                param.Add("@BookCategoryIdList", categoryList);
            }
        }

		if (bookFormatList != null && bookFormatList.Count != 0)
        {
            if (bookFormatList.Count == 1)
            {
                sbSql.Where("t.[BookFormat]=@BookFormat");
                param.Add("@BookFormat", bookFormatList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.[BookFormat] IN @BookFormat");
                param.Add("@BookFormatList", bookFormatList);
            }
        }

        if (isEBook.HasValue)
        {
            sbSql.Where("t.IsEBook=@IsEBook");
            param.Add("@IsEBook", isEBook.Value);
        }
        #endregion

        sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.Id=t.BookId");
        sbSql.LeftJoin($"{BookCategory.MsSqlTable} bc ON bc.Id=b.BookCategoryId");
        sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.Id=b.AuthorId");
		sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.Id=b.CoAuthorId");
		sbSql.LeftJoin($"{UserBook.MsSqlTable} ub ON ub.Id=t.UserBookId");

		sbSql.OrderBy("t.PurchasedDate DESC");
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
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.*, b.*, bc.*, auth.*, coAuth.*, ub.* FROM {DbObject.MsSqlTable} t " +
                $"INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
                
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<BookPurchaseHistory, Book, BookCategory, Person, Person, UserBook, BookPurchaseHistory>(
							sql, (bph, book, category, author, coAuthor, userBook) =>
							{
								if (book != null)
								{
									book.Category = category;
									book.Author = author;
									book.CoAuthor = coAuthor;
									bph.Book = book;
								}

								bph.UserBook = userBook;

								return bph;
							}, param, splitOn: "Id")).AsList();

		return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
		List<int>? userIdList = null,
		DateTime? purchasedDateFrom = null,
        DateTime? purchasedDateTo = null,
        decimal? unitPriceFrom = null,
        decimal? unitPriceTo = null,
        string? bookTitle = null,
        string? authorName = null,
        List<int>? categoryList = null,
        List<string>? bookFormatList = null,
        bool? isEBook = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
		{
			sbSql.Where("(t.ObjectCode LIKE '%'+@ObjectCode+'%' OR b.ISBN13 LIKE '%'+@ObjectCode+'%' OR b.ISBN10 LIKE '%'+@ObjectCode+'%')");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(objectName))
		{
			sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
			param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
		}

		if (userIdList is not null && userIdList.Count != 0)
		{
			if (userIdList.Count == 1)
			{
				sbSql.Where("ub.UserId=@UserId");
				param.Add("@UserId", userIdList[0]);
			}
			else
			{
				sbSql.Where("ub.UserId IN @UserIdList");
				param.Add("@UserIdList", userIdList);
			}
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
			sbSql.Where("t.PurchasedDate<=@PurchasedDateTo");
			param.Add("@PurchasedDateTo", purchasedDateTo.Value);
		}

		if (unitPriceFrom.HasValue)
		{
			sbSql.Where("t.UnitPrice IS NOT NULL");
			sbSql.Where("t.UnitPrice>=@UnitPriceFrom");
			param.Add("@UnitPriceFrom", unitPriceFrom.Value);

			if (unitPriceTo.HasValue)
			{
				sbSql.Where("t.UnitPrice<=@UnitPriceTo");
				param.Add("@UnitPriceTo", unitPriceTo.Value);
			}
		}
		else if (unitPriceTo.HasValue)
		{
			sbSql.Where("t.UnitPrice IS NOT NULL");
			sbSql.Where("t.UnitPrice<=@UnitPriceTo");
			param.Add("@UnitPriceTo", unitPriceTo.Value);
		}

		if (!string.IsNullOrEmpty(bookTitle))
		{
			sbSql.Where("UPPER(b.[Title]) LIKE '%'+UPPER(@BookTitle)+'%'");
			param.Add("@BookTitle", bookTitle, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(authorName))
		{
			Regex engName = new(@"^[a-zA-Z., ]{1,}$");

			if (engName.IsMatch(authorName))
			{
				sbSql.Where("((auth.ObjectName Is NOT NULL AND LOWER(ISNULL(auth.GivenName,'') + ' ' + ISNULL(auth.Surname,'')) LIKE '%'+LOWER(@AuthorName)+'%') OR " +
					"(coAuth.ObjectName Is NOT NULL AND LOWER(ISNULL(coAuth.GivenName,'') + ' ' + ISNULL(coAuth.Surname,'')) LIKE '%'+LOWER(@AuthorName)+'%'))");
				param.Add("@AuthorName", authorName, DbType.AnsiString);
			}
			else
			{
				sbSql.Where("((auth.ObjectName Is NOT NULL AND ISNULL(auth.SurnameKh,'') + ' ' + ISNULL(auth.GivenNameKh,'') LIKE '%'+@AuthorName+'%') OR " +
					"(coAuth.ObjectName Is NOT NULL AND ISNULL(coAuth.SurnameKh,'') + ' ' + ISNULL(coAuth.GivenNameKh,'') LIKE '%'+@AuthorName+'%'))");
				param.Add("@AuthorName", authorName);
			}
		}

		if (categoryList != null && categoryList.Count != 0)
		{
			if (categoryList.Count == 1)
			{
				sbSql.Where("b.BookCategoryId=@BookCategoryId");
				param.Add("@BookCategoryId", categoryList[0]);
			}
			else
			{
				sbSql.Where("b.BookCategoryId IN @BookCategoryIdList");
				param.Add("@BookCategoryIdList", categoryList);
			}
		}

		if (bookFormatList != null && bookFormatList.Count != 0)
		{
			if (bookFormatList.Count == 1)
			{
				sbSql.Where("t.[BookFormat]=@BookFormat");
				param.Add("@BookFormat", bookFormatList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.[BookFormat] IN @BookFormatList");
				param.Add("@BookFormatList", bookFormatList);
			}
		}

		if (isEBook.HasValue)
		{
			sbSql.Where("t.IsEBook=@IsEBook");
			param.Add("@IsEBook", isEBook.Value);
		}
		#endregion

		sbSql.LeftJoin($"{Book.MsSqlTable} b ON b.Id=t.BookId");
        sbSql.LeftJoin($"{BookCategory.MsSqlTable} bc ON bc.Id=b.BookCategoryId");
        sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.Id=b.AuthorId");
		sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.Id=b.CoAuthorId");
		sbSql.LeftJoin($"{UserBook.MsSqlTable} ub ON ub.Id=t.UserBookId");

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Book).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}