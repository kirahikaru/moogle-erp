namespace DataLayer.GlobalConstant.Pru;

public static class Currencies
{
    public const string USD = "USD";
    public const string KHR = "KHR";
	public const string LAK = "LAK";
	public const string MMK = "MMK";

	public static string GetDisplayText(string? currencyCode)
    {
        return currencyCode switch
        {
            USD => "US Dollar",
            KHR => "Khmer Riel",
            LAK => "Laos Kip",
            MMK => "Myanmar Kyat",
            _ => ""
        };
    }

    public static IEnumerable<DropdownSelectItem> GetForDropdownList()
	{
		return [
			new DropdownSelectItem { Key = USD, Value = GetDisplayText(USD) },
			new DropdownSelectItem { Key = KHR, Value = GetDisplayText(KHR) },
			new DropdownSelectItem { Key = LAK, Value = GetDisplayText(LAK) },
			new DropdownSelectItem { Key = MMK, Value = GetDisplayText(MMK) },
			];
	}
}
