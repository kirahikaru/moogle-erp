using DataLayer.GlobalConstant;
using DataLayer.Models.Library;
using DataLayer.Models.SystemCore.NonPersistent;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.Library;

public interface IBookRepos : IBaseRepos<Book>
{
	Task<Book?> GetFullAsync(int id);
	Task<int> InsertFullAsync(Book book);
	Task<bool> UpdateFullAsync(Book book);
    Task<int> InsertOrUpdateFullAsync(Book book);

	Task<bool> IsDuplicateIsbn13Async(int objId, string isbn13);
	Task<bool> IsDuplicateIsbn10Async(int objId, string isbn10);

	Task<List<Person>> GetAuthorByBookAsync(int bookId);

	Task<List<Book>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? title = null,
		string? seriesName = null,
		string? authorName = null,
		string? isbn13 = null,
		string? isbn10 = null,
		int? publishedYearFrom = null,
		int? publishedYearTo = null,
		string? publisherName = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? title = null,
		string? seriesName = null,
		string? authorName = null,
		string? isbn13 = null,
		string? isbn10 = null,
		int? publishedYearFrom = null,
		int? publishedYearTo = null,
		string? publisherName = null);

	Task<List<DropdownSelectItem>> GetForDropdownSelect1Async(string? searchText = null);
}

public class BookRepos(IConnectionFactory connectionFactory) : BaseRepos<Book>(connectionFactory, Book.DatabaseObject), IBookRepos
{
	public async Task<bool> IsDuplicateIsbn13Async(int objId, string isbn13)
    {
        SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id<>@Id");
		sbSql.Where("t.ISBN13 IS NOT NULL");
		sbSql.Where("t.ISBN13=@isbn13");

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        
        param.Add("@Id", objId);
        param.Add("@isbn13", isbn13, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;

        int count = await cn.ExecuteScalarAsync<int>(sql, param);

        return count > 0;
    }

    public async Task<bool> IsDuplicateIsbn10Async(int objId, string isbn10)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id<>@Id");
		sbSql.Where("t.ISBN10 IS NOT NULL");
		sbSql.Where("t.ISBN10=@isbn10");

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		
        param.Add("@Id", objId);
        param.Add("@isbn10", isbn10, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;

        int count = await cn.ExecuteScalarAsync<int>(sql, param);

        return count > 0;
    }

    public async Task<Book?> GetFullAsync(int id)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND Id=@Id; " +
                     $"SELECT b.* FROM {BookBookGenre.MsSqlTable} a LEFT JOIN {BookGenre.MsSqlTable} b ON b.Id=BookGenreId WHERE a.BookId=@Id; " +
                     $"SELECT * FROM {BookPurchaseHistory.MsSqlTable} WHERE IsDeleted=0 AND BookId=@Id; " +
                     $"SELECT p.* FROM {BookPersonRoleMap.MsSqlTable} bprm LEFT JOIN {Person.MsSqlTable} p ON p.Id=bprm.PersonId WHERE bprm.BookRole=@BookRole AND bprm.BookId=@Id";

        var param = new { Id = id, BookRole = BookRoles.AUTHOR };

        using var cn = ConnectionFactory.GetDbConnection()!;

        Book? data = null;

        using (var multi = await cn.QueryMultipleAsync(sql, param))
        {
            data = await multi.ReadSingleOrDefaultAsync<Book>();

            if (data != null)
            {
				data.Genres = (await multi.ReadAsync<BookGenre>()).AsList();
				data.PurchaseHistories = (await multi.ReadAsync<BookPurchaseHistory>()).AsList();
				data.Authors = (await multi.ReadAsync<Person>()).AsList();
			}
        }

        if (data != null)
        {
            if (data.BookCategoryId.HasValue)
            {
                string bookCtgSql = $"SELECT * FROM {BookCategory.MsSqlTable} WHERE Id=@BookCategoryId";
                data.Category = await cn.QuerySingleOrDefaultAsync<BookCategory>(bookCtgSql, new { BookCategoryId = data.BookCategoryId.Value });
            }

            if (data.AuthorId.HasValue)
            {
                string authorSql = $"SELECT * FROM {Person.MsSqlTable} WHERE Id=@AuthorId";
                data.Author = await cn.QuerySingleOrDefaultAsync<Person>(authorSql, new { AuthorId = data.AuthorId.Value });
            }

            if (data.CoAuthorId.HasValue)
            {
                string coAuthorSql = $"SELECT * FROM {Person.MsSqlTable} WHERE Id=@CoAuthorId";
                data.Author = await cn.QuerySingleOrDefaultAsync<Person>(coAuthorSql, new { CoAuthorId = data.CoAuthorId.Value });
            }
        }

        return data;
    }

    public async Task<int> InsertFullAsync(Book book)
    {
        ArgumentNullException.ThrowIfNull("Book passed is null.");

        using var cn = ConnectionFactory.GetDbConnection()!;

        //<**!IMPORATNT**> Need to ensure Connection is open before opening transaction
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
			if (book.Authors.Count != 0)
            {
                book.AuthorId = book.Authors[0].Id;
                if (book.Authors.Count > 1)
                    book.AuthorId = book.Authors[1].Id;
            }
            else
            {
                book.AuthorId = null;
                book.CoAuthorId = null;
            }

            int objId = await cn.InsertAsync(book, tran);

            if (objId > 0)
            {
                int genreInsCount = 0;
                int authorInsCount = 0;

				if (book.Genres.Count != 0)
                {
					List<BookBookGenre> bookGenres = [];
                    foreach (BookGenre genre in book.Genres)
                        bookGenres.Add(new BookBookGenre()
                        {
                            BookId = objId,
                            BookGenreId = genre.Id,
                            GenreName = genre.ObjectName
                        });

                    genreInsCount = await cn.InsertAsync(bookGenres, tran);
                }

				if (book.Authors.Count != 0)
                {
                    List<BookPersonRoleMap> bookPersonRoles = [];
                    foreach (Person author in book.Authors)
                    {
                        if (book.AuthorId == null)
                            book.AuthorId = author.Id;
						else book.CoAuthorId ??= author.Id;

                        bookPersonRoles.Add(new BookPersonRoleMap()
                        {
                            BookId = objId,
                            PersonId = author.Id,
                            BookRole = BookRoles.AUTHOR
                        });
                    }

                    authorInsCount = await cn.InsertAsync(bookPersonRoles, tran);
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

    public async Task<bool> UpdateFullAsync(Book book)
    {
        ArgumentNullException.ThrowIfNull("Book passed is null.");

        using var cn = ConnectionFactory.GetDbConnection()!;

        //<**!IMPORATNT**> Need to ensure Connection is open before opening transaction
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            if (book.Authors.Count != 0)
            {
                book.AuthorId = book.Authors[0].Id;
                if (book.Authors.Count > 1)
                    book.AuthorId = book.Authors[1].Id;
            }
            else
            {
                book.AuthorId = null;
                book.CoAuthorId = null;
            }

            bool isUpdated = await cn.UpdateAsync(book, tran);

            if (isUpdated)
            {
                int genreInsCount = 0;
                int authorInsCount = 0;

                string delBookBookGenreSql = $"DELETE FROM {BookBookGenre.MsSqlTable} WHERE BookId=@BookId";
                string delBookPersonRoleMapSql = $"DELETE FROM {BookPersonRoleMap.MsSqlTable} WHERE BookId=@BookId AND BookRole=@BookRole";

                await cn.ExecuteAsync(delBookBookGenreSql, new { BookId = book.Id }, transaction: tran);
                await cn.ExecuteAsync(delBookPersonRoleMapSql, new { BookId = book.Id, BookRole = new DbString { Value = BookRoles.AUTHOR, IsAnsi = true } }, transaction: tran);

				if (book.Genres.Count != 0)
                {
                    List<BookBookGenre> bookGenres = [];
                    foreach (BookGenre genre in book.Genres)
                        bookGenres.Add(new BookBookGenre()
                        {
                            BookId = book.Id,
                            BookGenreId = genre.Id,
                            GenreName = genre.ObjectName
                        });

                    genreInsCount = await cn.InsertAsync(bookGenres, tran);
                }

                if (book.Authors.Any())
                {
                    List<BookPersonRoleMap> bookPersonRoles = [];
                    foreach (Person author in book.Authors)
                        bookPersonRoles.Add(new BookPersonRoleMap()
                        {
                            BookId = book.Id,
                            PersonId = author.Id,
                            BookRole = BookRoles.AUTHOR
                        });

                    authorInsCount = await cn.InsertAsync(bookPersonRoles, tran);
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

	public async Task<int> InsertOrUpdateFullAsync(Book book)
    {
		using var cn = ConnectionFactory.GetDbConnection()!;

		//<**!IMPORATNT**> Need to ensure Connection is open before opening transaction
		if (cn.State != ConnectionState.Open) cn.Open();

		using var tran = cn.BeginTransaction();

        try
        {
            int objId = 0;

            if (book.Id > 0)
            {
				bool isUpd = await cn.UpdateAsync(book, tran);

                if (isUpd)
                {
                    objId = book.Id;

					int genreInsCount = 0;
					int authorInsCount = 0;

					string delBookBookGenreSql = $"DELETE FROM {BookBookGenre.MsSqlTable} WHERE BookId=@BookId";
					string delBookPersonRoleMapSql = $"DELETE FROM {BookPersonRoleMap.MsSqlTable} WHERE BookId=@BookId AND BookRole=@BookRole";

					await cn.ExecuteAsync(delBookBookGenreSql, new { BookId = book.Id }, transaction: tran);
					await cn.ExecuteAsync(delBookPersonRoleMapSql, new { BookId = book.Id, BookRole = new DbString { Value = BookRoles.AUTHOR, IsAnsi = true } }, transaction: tran);

					if (book.Genres.Count != 0)
					{
						List<BookBookGenre> bookGenres = [];
						foreach (BookGenre genre in book.Genres)
							bookGenres.Add(new BookBookGenre()
							{
								BookId = book.Id,
								BookGenreId = genre.Id,
								GenreName = genre.ObjectName
							});

						genreInsCount = await cn.InsertAsync(bookGenres, tran);
					}

					if (book.Authors.Any())
					{
						List<BookPersonRoleMap> bookPersonRoles = [];
						foreach (Person author in book.Authors)
							bookPersonRoles.Add(new BookPersonRoleMap()
							{
								BookId = book.Id,
								PersonId = author.Id,
								BookRole = BookRoles.AUTHOR
							});

						authorInsCount = await cn.InsertAsync(bookPersonRoles, tran);
					}
				}
			}
            else
            {
                objId = await cn.InsertAsync(book, tran);

				int genreInsCount = 0;
				int authorInsCount = 0;

				if (book.Genres.Count != 0)
				{
					List<BookBookGenre> bookGenres = [];
					foreach (BookGenre genre in book.Genres)
						bookGenres.Add(new BookBookGenre()
						{
							BookId = objId,
							BookGenreId = genre.Id,
							GenreName = genre.ObjectName
						});

					genreInsCount = await cn.InsertAsync(bookGenres, tran);
				}

				if (book.Authors.Count != 0)
				{
					List<BookPersonRoleMap> bookPersonRoles = [];
					foreach (Person author in book.Authors)
					{
						if (book.AuthorId == null)
							book.AuthorId = author.Id;
						else book.CoAuthorId ??= author.Id;

						bookPersonRoles.Add(new BookPersonRoleMap()
						{
							BookId = objId,
							PersonId = author.Id,
							BookRole = BookRoles.AUTHOR
						});
					}

					authorInsCount = await cn.InsertAsync(bookPersonRoles, tran);
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

	public override async Task<KeyValuePair<int, IEnumerable<Book>>> SearchNewAsync(
        int pgSize = 0, int pgNo = 0, string? searchText = null, 
        IEnumerable<SqlSortCond>? sortConds = null, IEnumerable<SqlFilterCond>? filterConds = null, 
        List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (Regex.IsMatch(searchText, @"^[0-9]{13}$"))
			{
				sbSql.Where("t.ISBN13=@SearchText");
				param.Add("@SearchText", searchText);
			}
			else if (Regex.IsMatch(searchText, @"^[0-9]{10}$"))
			{
				sbSql.Where("t.ISBN10=@SearchText");
				param.Add("@SearchText", searchText);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.Title) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.AuthorText) LIKE '%'+UPPER(@SearchText)+'%')");
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

		sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.IsDeleted=0 AND auth.Id=t.AuthorId");
		sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.IsDeleted=0 AND coAuth.Id=t.CoAuthorId");
		sbSql.LeftJoin($"{BookCategory.MsSqlTable} bctg ON bctg.IsDeleted=0 AND bctg.Id=t.BookCategoryId");

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

        var dataList = await cn.QueryAsync<Book, Person, Person, BookCategory, Book>(sql, (book, author, coAuthor, bookCategory) =>
        {
            if (bookCategory != null)
                book.Category = bookCategory;

            book.Author = author;
            book.CoAuthor = coAuthor;
            return book;
        }, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

    public override async Task<List<Book>> QuickSearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? searchText = null,
        List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            //string alphabetsPattern = @"^[a-zA-Z ]{1,}$";
            //string numberPattern = @"^[0-9\-]{1,}$";
			string regexisbn13Pattern = @"^[0-9]{13}$";
			string regexisbn10Pattern = @"^[0-9]{10}$";

			if (!string.IsNullOrEmpty(searchText))
            {
                if (searchText.StartsWith("isbn:", StringComparison.OrdinalIgnoreCase))
                {
					sbSql.Where("t.ISBN13 LIKE '%'+@Isbn13+'%'");
					param.Add("@Isbn13", searchText.Replace("isbn:","", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);

					
                }
                else if (searchText.StartsWith("series:", StringComparison.OrdinalIgnoreCase))
                {
					sbSql.Where("t.SeriesName IS NOT NULL");
					sbSql.Where("UPPER(t.SeriesName) LIKE '%'+UPPER(@SearchText)+'%'");

					param.Add("@SearchText", searchText.Replace("series:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
				}
                else if (Regex.IsMatch(searchText, regexisbn13Pattern))
                {
					sbSql.Where("t.ISBN13=@Isbn13");
					param.Add("@Isbn13", searchText, DbType.AnsiString);
				}
				else if (Regex.IsMatch(searchText, regexisbn10Pattern))
				{
					sbSql.Where("t.ISBN10=@Isbn10");
					param.Add("@Isbn10", searchText, DbType.AnsiString);
				}
				else
                {
                    sbSql.Where("UPPER(t.Title) LIKE '%'+UPPER(@Title)+'%'");
                    param.Add("@Title", searchText, DbType.AnsiString);
                }
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.IsDeleted=0 AND auth.Id=t.AuthorId");
        sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.IsDeleted=0 AND coAuth.Id=t.CoAuthorId");
        sbSql.LeftJoin($"{BookCategory.MsSqlTable} bctg ON bctg.IsDeleted=0 AND bctg.Id=t.BookCategoryId");

        sbSql.OrderBy("t.Title ASC");

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
                $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<Book> result = (await cn.QueryAsync<Book, Person, Person, BookCategory, Book>(sql,
                                        (obj, auth, coAuth, bookCtg) =>
                                        {
                                            if (auth != null)
                                                obj.Author = auth;

                                            if (coAuth != null)
                                                obj.CoAuthor = coAuth;

                                            if (bookCtg != null)
                                                obj.Category = bookCtg;

                                            return obj;
                                        }, param: param, splitOn: "Id")).AsList();

        return result;
    }

	public override List<string> GetSearchOrderbBy()
	{
		return ["t.Title ASC"];
	}

	public override async Task<DataPagination> GetQuickSearchPaginationAsync(
        int pgSize = 0, string? searchText = null,
        List<int>? excludeIdList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
            //string alphabetsPattern = @"^[a-zA-Z ]{1,}$";
			//string numbersPattern = @"^[0-9\-]{1,}$";
			string regexisbn13Pattern = @"^[0-9]{13}$";
			string regexisbn10Pattern = @"^[0-9]{10}$";

			if (!string.IsNullOrEmpty(searchText))
			{
				if (searchText.StartsWith("isbn:", StringComparison.OrdinalIgnoreCase))
				{
					sbSql.Where("t.ISBN13 LIKE '%'+@Isbn13+'%'");
					param.Add("@Isbn13", searchText.Replace("isbn:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);


				}
				else if (searchText.StartsWith("series:", StringComparison.OrdinalIgnoreCase))
				{
					sbSql.Where("t.SeriesName IS NOT NULL");
					sbSql.Where("UPPER(t.SeriesName) LIKE '%'+UPPER(@SearchText)+'%'");

					param.Add("@SearchText", searchText.Replace("series:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
				}
				else if (Regex.IsMatch(searchText, regexisbn13Pattern))
				{
					sbSql.Where("t.ISBN13=@Isbn13");
					param.Add("@Isbn13", searchText, DbType.AnsiString);
				}
				else if (Regex.IsMatch(searchText, regexisbn10Pattern))
				{
					sbSql.Where("t.ISBN10=@Isbn10");
					param.Add("@Isbn10", searchText, DbType.AnsiString);
				}
				else
				{
					sbSql.Where("UPPER(t.Title) LIKE '%'+UPPER(@Title)+'%'");
					param.Add("@Title", searchText, DbType.AnsiString);
				}
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
            ObjectType = typeof(Book).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<Person>> GetAuthorByBookAsync(int bookId)
    {
        string sql = $"SELECT p.* FROM {BookPersonRoleMap.MsSqlTable} bprm LEFT JOIN {Person.MsSqlTable} p ON p.Id=bprm.PersonId WHERE bprm.BookId=@BookId";

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<Person> result = (await cn.QueryAsync<Person>(sql, new { BookId = bookId })).AsList();

        return result;
    }

    public async Task<List<Book>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? title = null,
        string? seriesName = null,
        string? authorName = null,
        string? isbn13 = null,
        string? isbn10 = null,
        int? publishedYearFrom = null,
        int? publishedYearTo = null,
        string? publisherName = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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
            sbSql.Where("t.ObjectName LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(title))
        {
            sbSql.Where("LOWER(t.[Title]) LIKE '%'+@Title+'%'");
            param.Add("@Title", title.ToLower());
        }

        if (!string.IsNullOrEmpty(seriesName))
        {
            sbSql.Where("LOWER(t.[SeriesName]) LIKE '%'+@SeriesName+'%'");
            param.Add("@SeriesName", seriesName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(isbn13))
        {
            sbSql.Where("UPPER(t.[ISBN13]) LIKE '%'+@ISBN13+'%'");
            param.Add("@ISBN13", isbn13.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(isbn10))
        {
            sbSql.Where("UPPER(t.[ISBN10]) LIKE '%'+@ISBN10+'%'");
            param.Add("@ISBN10", isbn10.ToUpper(), DbType.AnsiString);
        }

        if (publishedYearFrom.HasValue)
        {
            sbSql.Where("t.PublishedYear IS NOT NULL");
            sbSql.Where("t.PublishedYear>=@PublishedYearFrom");
            param.Add("@PublishedYearFrom", publishedYearFrom.Value);

            if (publishedYearTo.HasValue)
            {
                sbSql.Where("t.PublishedYear<=@PublishedYearTo");
                param.Add("@PublishedYearTo", publishedYearTo.Value);
            }
        }
        else if (publishedYearTo.HasValue)
        {
            sbSql.Where("t.PublishedYear IS NOT NULL");
            sbSql.Where("t.PublishedYear<=@PublishedYearTo");
            param.Add("@PublishedYearTo", publishedYearTo.Value);
        }

        if (!string.IsNullOrEmpty(publisherName))
        {
            sbSql.Where("t.PublisherName LIKE '%'+@PublisherName+'%'");
            param.Add("@PublisherName", publisherName, DbType.AnsiString);
        }
        #endregion

        using var cn = ConnectionFactory.GetDbConnection()!;

        if (!string.IsNullOrEmpty(authorName))
        {
            string[] authorNameParts = authorName.Split(' ');
            string findBookQry;
			List<int> bookIdList = [];

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

            sbSql.Where("t.Id IN @BookIdList");
            param.Add("@BookIdList", bookIdList);
        }

        sbSql.LeftJoin($"{Person.MsSqlTable} auth ON auth.IsDeleted=0 AND auth.Id=t.AuthorId");
        sbSql.LeftJoin($"{Person.MsSqlTable} coAuth ON coAuth.IsDeleted=0 AND coAuth.Id=t.CoAuthorId");
        sbSql.LeftJoin($"{BookCategory.MsSqlTable} bctg ON bctg.IsDeleted=0 AND bctg.Id=t.BookCategoryId");

        sbSql.OrderBy("t.[Title] ASC");

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
                $"SELECT * FROM {DbObject.MsSqlTable} t " +
                $"INNER JOIN pg p ON p.Id=i.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        List<Book> result = (await cn.QueryAsync<Book, BookCategory, Person, Person, Book>(sql, (book, bookCategory, author, coAuthor) => 
                                    {
                                        if (bookCategory != null)
                                            book.Category = bookCategory;

                                        book.Author = author;
                                        book.CoAuthor = coAuthor;
                                        return book;
                                    }, param, splitOn: "Id")).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? title = null,
        string? seriesName = null,
        string? authorName = null,
        string? isbn13 = null,
        string? isbn10 = null,
        int? publishedYearFrom = null,
        int? publishedYearTo = null,
        string? publisherName = null)
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
            sbSql.Where("t.ObjectName LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(title))
        {
            sbSql.Where("LOWER(t.[Title]) LIKE '%'+@Title+'%'");
            param.Add("@Title", title.ToLower());
        }

        if (!string.IsNullOrEmpty(seriesName))
        {
            sbSql.Where("LOWER(t.[SeriesName]) LIKE '%'+@SeriesName+'%'");
            param.Add("@SeriesName", seriesName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(isbn13))
        {
            sbSql.Where("UPPER(t.[ISBN13]) LIKE '%'+@ISBN13+'%'");
            param.Add("@ISBN13", isbn13.ToUpper(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(isbn10))
        {
            sbSql.Where("UPPER(t.[ISBN10]) LIKE '%'+@ISBN10+'%'");
            param.Add("@ISBN10", isbn10.ToUpper(), DbType.AnsiString);
        }

        if (publishedYearFrom.HasValue)
        {
            sbSql.Where("t.PublishedYear IS NOT NULL");
            sbSql.Where("t.PublishedYear>=@PublishedYearFrom");
            param.Add("@PublishedYearFrom", publishedYearFrom.Value);

            if (publishedYearTo.HasValue)
            {
                sbSql.Where("t.PublishedYear<=@PublishedYearTo");
                param.Add("@PublishedYearTo", publishedYearTo.Value);
            }
        }
        else if (publishedYearTo.HasValue)
        {
            sbSql.Where("t.PublishedYear IS NOT NULL");
            sbSql.Where("t.PublishedYear<=@PublishedYearTo");
            param.Add("@PublishedYearTo", publishedYearTo.Value);
        }

        if (!string.IsNullOrEmpty(publisherName))
        {
            sbSql.Where("t.PublisherName LIKE '%'+@PublisherName+'%'");
            param.Add("@PublisherName", publisherName, DbType.AnsiString);
        }
        #endregion

        using var cn = ConnectionFactory.GetDbConnection()!;

        if (!string.IsNullOrEmpty(authorName))
        {
            string[] authorNameParts = authorName.Split(' ');
            string findBookQry;
            List<int> bookIdList = [new()];

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

            sbSql.Where("t.Id IN @BookIdList");
            param.Add("@BookIdList", bookIdList);
        }

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
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

    public async Task<List<DropdownSelectItem>> GetForDropdownSelect1Async(string? searchText = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("t.Id")
            .Select("'Key'=t.ObjectCode")
            .Select("'Value'=t.Title + ' by ' +ISNULL(p.ObjectName, '???') COLLATE DATABASE_DEFAULT");

        sbSql.LeftJoin($"{Person.MsSqlTable} p ON p.Id=t.AuthorId");

        sbSql.Where("t.IsDeleted=0");

        sbSql.OrderBy("t.Title");
        
        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("(UPPER(t.Title) LIKE '%'+UPPER(@SearchText)+'%' OR t.ISBN13 LIKE @SearchText+'%')");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<DropdownSelectItem> dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return dataList;
    }
}