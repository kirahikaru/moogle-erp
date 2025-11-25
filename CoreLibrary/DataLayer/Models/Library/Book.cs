using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Library;
[Table("[lib].[Book]")]
public class Book : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.LIBRARY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Book).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "book";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required, MaxLength(255)]
    public string? Title { get; set; }

    public int? AuthorId { get; set; }
    public int? CoAuthorId { get; set; }
    public int? BookCategoryId { get; set; }

    [MaxLength(150)]
    public string? SeriesName { get; set; }

    [Range(0, 9999999, ErrorMessage = "Invalid 'Book #' format. It must be positive number.")]
    public decimal? BookNo { get; set; }

    [MaxLength(13)]
    public string? ISBN13 { get; set; }

    [MaxLength(10)]
    public string? ISBN10 { get; set; }

	public string? AuthorText { get; set; }

	[Range(0, 9999, ErrorMessage ="Invalid 'Published Year' format. Please input positive whole number.")]
    public int? PublishedYear { get; set; }

    [Range(0, 9999, ErrorMessage = "Invalid 'Released Year' format. Please input positive whole number.")]
    public int? ReleasedYear { get; set; }

    /// <summary>
    /// Valid Values > GlobalConstants_LIB.BookPrintFormats
    /// </summary>
    public string? PrintFormat { get; set; }

    [MaxLength(150)]
    public string? PublisherName { get; set; }

    [MaxLength(255)]
    public string? CoverImagePath { get; set; }

    [MaxLength(100)]
    public string? BookVersion { get; set; }

    [MaxLength(100)]
    public string? BookEdition { get; set; }

    [DataType(DataType.Url)]
    [MaxLength(255)]
    public string? InfoUrl { get; set; }

    [DataType(DataType.Url)]
    [MaxLength(255)]
    public string? PurchaseUrl { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Person? Author { get; set; }

	[Computed, Write(false)]
	public Person? CoAuthor { get; set; }

	[Computed, Write(false)]
	public List<Person> Authors { get; set; }

	[Computed, Write(false)]
	public List<BookGenre> Genres { get; set; }

	[Computed, Write(false)]
	public BookCategory? Category { get; set; }
	#endregion

	[Computed, Write(false)]
	public List<BookPurchaseHistory> PurchaseHistories { get; set; }

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string BookNoText
	{
		get
		{
            if (BookNo != null)
            {
                if (BookNo % 1.0m > 0)
                    return BookNo!.Value.ToString("#0.00");
                else
                    return BookNo!.Value.ToString("#,##0");
			}

			return "";
		}
	}

	[Computed, Write(false), ReadOnly(true)]
	public string AuthorNames
    {
        get {
            StringBuilder sb = new();

            if (Authors.Any())
            {
                foreach (Person p in Authors)
                {
                    if (sb.Length > 0)
                        sb.Append($", {p.FullNameEnText.Trim()}");
                    else
                        sb.Append(p.FullNameEnText.Trim());
                }
            }

            return sb.ToString();
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string AuthorDisplayNames
    {
        get
        {
            StringBuilder sb = new();

			if (Authors.Count != 0)
            {
                foreach (Person p in Authors)
                {
                    if (sb.Length > 0)
                        sb.Append($", {p.ObjectName!.Trim()}");
                    else
                        sb.Append(p.ObjectName!.Trim());
                }
            }

            return sb.ToString();
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string AuthorNameText
    {
        get
        {
            StringBuilder sb = new();
            if (Author != null)
                sb.Append(Author.ObjectName);

            if (CoAuthor != null)
                sb.Append(", " + CoAuthor.ObjectName);

            return sb.ToString();
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string AuthorNameList
    {
        get {
            string authors = "";

            if (Authors != null && Authors.Any())
            {
                for (int i = 0; i < Authors.Count; i++)
                {
                    if (i > 0)
                        authors += (Environment.NewLine + Authors[i].FullNameEnText);
                    else
                        authors += Authors[i].FullNameEnText;
                }

                return authors;
            }
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string GenreList
    {
        get {
            string genres = "";

            if (Genres != null && Genres.Any())
            {
                for (int i = 0; i < Genres.Count; i++)
                {
                    if (i > 0)
                        genres += (", " + Genres[i].ObjectName);
                    else
                        genres += Genres[i].ObjectName;
                }

                return genres;
            }
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string SeriesText => !string.IsNullOrEmpty(SeriesName) ? SeriesName + (BookNo.HasValue ? $" (#{BookNo.Value:#0.##})" : "") : "-";

	[Computed, Write(false), ReadOnly(true)]
	public string PrintFormatText => BookPrintFormats.GetDisplayText(PrintFormat);
    #endregion

    public Book() : base()
    {
        Authors = [];
        Genres = [];
        PurchaseHistories = [];
    }
}