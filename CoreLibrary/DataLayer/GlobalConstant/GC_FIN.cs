namespace DataLayer.GlobalConstant;

#region FIN - Financial Management

public static class CurrencySymbols
{
    public const string KHMER_RIEL = "៛";
    public const string US_DOLLAR = "$";
    public const string THAI_BAHT = "฿";
    public const string VIETNAM_DONG = "₫";
}

public static class Currencies
{
    public const string US_USD = "USD";
    public const string CAMBODIA_KHR = "KHR";
    public const string THAI_THB = "THB";
    public const string VIETNAM_VND = "VND";
    public const string EUROPE_EUR = "EUR";
    public const string SINGAPORE_SGD = "SGD";

    public static string CurrencyDisplayFormat(string? currencyCode)
    {
        return currencyCode switch
        {
            CAMBODIA_KHR => "#,##0",
            THAI_THB => "#,##0",
            VIETNAM_VND => "#,##0",
            _ => "#,##0.00"
        };
    }

    public static bool HasNoDecimalDisplay(string currencyCode)
    {
        return currencyCode switch
        {
            CAMBODIA_KHR => true,
            THAI_THB => true,
            VIETNAM_VND => true,
            _ => false
        };
    }

    public static List<string> GetNoDecimalCurrencies()
    {
        return [CAMBODIA_KHR, THAI_THB, VIETNAM_VND];
    }

    public static string GetSymbol(string? currencyCode)
    {
        return currencyCode switch
        {
            US_USD => "$",
            CAMBODIA_KHR => "៛",
            THAI_THB => "฿",
            VIETNAM_VND => "₫",
            EUROPE_EUR => "€",
            SINGAPORE_SGD => "S$",
            _ => ""
        };
    }

    public static string GetDisplayText(string? code)
    {
        return code switch
        {
            US_USD => "US Dollar",
            CAMBODIA_KHR => "Khmer Riel",
            THAI_THB => "Thai Baht",
            VIETNAM_VND => "Vietnam Dong",
            EUROPE_EUR => "Euro",
            SINGAPORE_SGD => "Singapore Dollar",
            _ => "",
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { CAMBODIA_KHR, GetDisplayText(CAMBODIA_KHR) },
            { EUROPE_EUR, GetDisplayText(EUROPE_EUR) },
            { SINGAPORE_SGD, GetDisplayText(SINGAPORE_SGD) },
            { THAI_THB, GetDisplayText(THAI_THB) },
            { US_USD, GetDisplayText(US_USD) },
            { VIETNAM_VND, GetDisplayText(VIETNAM_VND) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem() { Key = CAMBODIA_KHR, Value = GetDisplayText(CAMBODIA_KHR) },
            new DropdownSelectItem() { Key = EUROPE_EUR, Value = GetDisplayText(EUROPE_EUR) },
            new DropdownSelectItem() { Key = SINGAPORE_SGD, Value = GetDisplayText(SINGAPORE_SGD) },
            new DropdownSelectItem() { Key = THAI_THB, Value = GetDisplayText(THAI_THB) },
            new DropdownSelectItem() { Key = US_USD, Value = GetDisplayText(US_USD) },
            new DropdownSelectItem() { Key = VIETNAM_VND, Value = GetDisplayText(VIETNAM_VND) }
        ];
        return list;
    }
}

public static class BankTypes
{
    public const string COMMERCIAL = "COMMERCIAL";
    public const string SPECIALIZED = "SPECIALIZED";
    public const string MFI_DEPOSIT = "MFI_DEPOSIT";
    public const string MFI_NO_DEPOSIT = "MFI_NO_DEPOSIT";


    public static string GetDisplayText(string? bankType)
    {
        return bankType switch
        {
            COMMERCIAL => "Commercial Bank",
            SPECIALIZED => "Specialized Bank",
            MFI_DEPOSIT => "MicroFinance (Deposit Taking)",
            MFI_NO_DEPOSIT => "MicroFinance (Non-Deposit Taking)",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { COMMERCIAL, GetDisplayText(COMMERCIAL) },
            { SPECIALIZED, GetDisplayText(SPECIALIZED) },
            { MFI_DEPOSIT, GetDisplayText(MFI_DEPOSIT) },
            { MFI_NO_DEPOSIT, GetDisplayText(MFI_NO_DEPOSIT) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = COMMERCIAL, Value = GetDisplayText(COMMERCIAL) },
            new DropdownSelectItem { Key = SPECIALIZED, Value = GetDisplayText(SPECIALIZED) },
            new DropdownSelectItem { Key = MFI_DEPOSIT, Value = GetDisplayText(MFI_DEPOSIT) },
            new DropdownSelectItem { Key = MFI_NO_DEPOSIT, Value = GetDisplayText(MFI_NO_DEPOSIT) }
        ];

        return list;
    }
}

public static class CustomerStatuses
{
    public const string PENDING_ACTIVATION = "PENDING-ACTIVATION";
	public const string ACTIVE = "ACTIVE";
    public const string INACTIVE = "INACTIVE";
    public const string TERMINATED = "TERMINATED";
    public const string BLACKLISTED = "BLACKLIST";

    public static bool IsValid(string customerStatus)
    {
        return customerStatus switch
        {
            PENDING_ACTIVATION or ACTIVE or INACTIVE or TERMINATED or BLACKLISTED => true,
            _ => false,
        };
    }

    public static string GetDisplayText(string? customerStatus)
    {
        return customerStatus switch
        {
            PENDING_ACTIVATION => "Pending Activation",
            ACTIVE => "Active",
            INACTIVE => "Inactive",
            BLACKLISTED => "Blacklisted",
            TERMINATED => "Terminated",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
			{ PENDING_ACTIVATION, GetDisplayText(PENDING_ACTIVATION) },
			{ ACTIVE, GetDisplayText(ACTIVE) },
            { INACTIVE, GetDisplayText(INACTIVE) },
            { BLACKLISTED, GetDisplayText(BLACKLISTED) },
			{ TERMINATED, GetDisplayText(TERMINATED) }
	    };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = ACTIVE, Value = GetDisplayText(ACTIVE) },
            new DropdownSelectItem { Key = INACTIVE, Value = GetDisplayText(INACTIVE) },
            new DropdownSelectItem { Key = BLACKLISTED, Value = GetDisplayText(BLACKLISTED) },
			new DropdownSelectItem { Key = TERMINATED, Value = GetDisplayText(TERMINATED) }
		];

        return list;
    }
}

public static class CustomerTypes
{
    public const string INDIVIDUAL = "I";
    public const string BUSINESS_ENTITY = "B";
    public const string COMPANY = "C";
    public const string ORGANIZATION = "O";

    public static bool IsValid(string customerType)
    {
        return customerType switch
        {
            INDIVIDUAL => true,
            BUSINESS_ENTITY => true,
            COMPANY => true,
            ORGANIZATION => true,
            _ => false
        };
    }

    public static string GetDisplayText(string? customerType)
    {
        return customerType switch
        {
            INDIVIDUAL => "Individual Person",
            BUSINESS_ENTITY => "Business Entity",
            COMPANY => "Company",
            ORGANIZATION => "Organization",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { INDIVIDUAL, GetDisplayText(INDIVIDUAL) },
            { BUSINESS_ENTITY, GetDisplayText(BUSINESS_ENTITY) },
            { COMPANY, GetDisplayText(COMPANY) },
            { ORGANIZATION, GetDisplayText(ORGANIZATION) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = INDIVIDUAL, Value = GetDisplayText(INDIVIDUAL) },
            new DropdownSelectItem { Key = BUSINESS_ENTITY, Value = GetDisplayText(BUSINESS_ENTITY) },
            new DropdownSelectItem { Key = COMPANY, Value = GetDisplayText(COMPANY) },
            new DropdownSelectItem { Key = ORGANIZATION, Value = GetDisplayText(ORGANIZATION) }
        ];

        return list;
    }
}
#endregion
