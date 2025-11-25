using DataLayer.Models;

namespace DataLayer.GlobalConstant;

public static class BookRoles
{
    public const string AUTHOR = "AUTHOR";
    public const string CO_AUTHOR = "CO-AUTHOR";

    public static string GetDisplayText(string? bookRole)
    {
        return bookRole switch
        {
            AUTHOR => "Author",
            CO_AUTHOR => "Co-Author",
            _ => ""
        };
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
        return new()
        {
            new DropdownSelectItem() { Key = AUTHOR, Value = GetDisplayText(AUTHOR) },
            new DropdownSelectItem() { Key = CO_AUTHOR, Value = GetDisplayText(CO_AUTHOR) }
        };
    }
}

public static class BookPrintFormats
{
    public const string HARD_COVER = "HARDCOVER";
    public const string PAPERBACK = "PAPERBACK";

    public static string GetDisplayText(string? bookPrintFormat)
    {
        return bookPrintFormat switch
        {
            HARD_COVER => "Hard Cover",
            PAPERBACK => "Paperback",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { HARD_COVER, GetDisplayText(HARD_COVER) },
            { PAPERBACK, GetDisplayText(PAPERBACK) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem() { Key = HARD_COVER, Value = GetDisplayText(HARD_COVER) },
            new DropdownSelectItem() { Key = PAPERBACK, Value = GetDisplayText(PAPERBACK) }
        ];

        return list;
    }
}

public static class UserBookOwnershipStatuses
{
    public const string AVAILABLE = "AVAILABLE";
    public const string WISHLIST = "WISHLIST";
    public const string LOAN = "LOAN";
    public const string LOST = "LOST";
    public const string GAVE_AWAY = "GAVE_AWAY";

    public static string GetDisplayText(string? bookRole)
    {
        return bookRole switch
        {
            AVAILABLE => "Available",
            WISHLIST => "Wishlist",
            LOAN => "Loan To Others",
            GAVE_AWAY => "Gave Away",
            LOST => "Lost",
            _ => ""
        };
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem() { Key = AVAILABLE, Value = GetDisplayText(AVAILABLE) },
            new DropdownSelectItem() { Key = WISHLIST, Value = GetDisplayText(WISHLIST) },
            new DropdownSelectItem() { Key = LOAN, Value = GetDisplayText(LOAN) },
            new DropdownSelectItem() { Key = LOST, Value = GetDisplayText(LOST) },
            new DropdownSelectItem() { Key = GAVE_AWAY, Value = GetDisplayText(GAVE_AWAY) }
        ];

        return list;
    }
}
