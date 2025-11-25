using Microsoft.IdentityModel.Tokens;

namespace DataLayer.GlobalConstant;
// public static class DocumentTypeCode {
//        public const string PROFILE = "PROFILE";
//        public const string PROVINCE = "PROVINCE";
//        public const string DISTRICT = "DISTRICT";
//}

public static class CRUDCModes
{
    public const string CREATE = "create";
	public const string CLONE = "clone";
	public const string READ = "read";
	public const string DELETE = "delete";
	public const string UPDATE = "update";

    public static bool IsValid(string mode)
    {
        return mode switch
        {
            CREATE => true,
			READ => true,
			UPDATE => true,
			DELETE => true,
			CLONE => true,
            _ => false
        };
    }
}

/// <summary>
/// Cambodia Country Structure Types
/// </summary>
public static class CambodiaCtyStructTypes
{
    public const string CAPITAL_CITY = "CAPITAL";
    public const string PROVINCE = "PROVINCE";

    public const string CITY = "CITY";
    public const string KHAN = "KHAN";
    public const string SROK = "SROK";
    public const string DISTRICT = "DISTRICT";

    public const string COMMUNE = "COMMUNE";
    public const string SANGKAT = "SANGKAT";

    public const string VILLAGE = "VILLAGE";
    public const string GROUP = "GROUP";


    public static string GetDisplayText(string? structureType)
    {
        return structureType switch
        {
            CAPITAL_CITY => "Capital City",
            PROVINCE => "Province",
            CITY => "City",
            KHAN => "Khan",
            SROK => "Srok",
            SANGKAT => "Sangkat",
            DISTRICT => "District",
            COMMUNE => "Commune",
            VILLAGE => "Village",
            _ => ""
        };
    }

    public static string GetDisplayKhmerText(string structureType)
    {
        return structureType switch
        {
            CAPITAL_CITY => "រាជធានី",
            CITY => "ក្រុង",
            PROVINCE => "ខេត្ត",
            DISTRICT => "ស្រុក/ខណ្ឌ",
            COMMUNE => "ឃុំ/សង្កាត់",
            GROUP => "ក្រុម",
            SANGKAT => "សង្កាត់",
            SROK => "ស្រុក",
            KHAN => "ខណ្ឌ",
            VILLAGE => "ភូមិ",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { CITY, GetDisplayText(CITY) },
            { PROVINCE, GetDisplayText(PROVINCE) },
            { DISTRICT, GetDisplayText(DISTRICT) },
            { COMMUNE, GetDisplayText(COMMUNE) },
            { VILLAGE, GetDisplayText(VILLAGE) }
        };

        return list;
    }

    public static Dictionary<string, string> GetAllKhmer()
    {
        Dictionary<string, string> list = new()
        {
            { CITY, GetDisplayKhmerText(CITY) },
            { PROVINCE, GetDisplayKhmerText(PROVINCE) },
            { DISTRICT, GetDisplayKhmerText(DISTRICT) },
            { COMMUNE, GetDisplayKhmerText(COMMUNE) },
            { VILLAGE, GetDisplayKhmerText(VILLAGE) }
        };

        return list;
    }
}

public static class CambodiaCommuneTypes
{
    public const string SANGKAT = "S";
    public const string COMMUNE = "C";

	public static string GetDisplayText(string? districtType)
	{
		return districtType switch
		{
			SANGKAT => "Sangkat",
			COMMUNE => "Commune",
			_ => ""
		};
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = COMMUNE, Value = GetDisplayText(COMMUNE) },
            new DropdownSelectItem { Key = SANGKAT, Value = GetDisplayText(SANGKAT) }
        ];

        return list;
    }
}

public static class CambodiaDistrictTypes
{
    public const string CITY = "C";
    public const string KHAN = "K";
    public const string SROK = "S";

    public static string GetDisplayText(string? districtType)
    {
        return districtType switch
        {
            CITY => "City",
            KHAN => "Khan",
            SROK => "Srok",
            _ => ""
        };
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = CITY, Value = GetDisplayText(CITY) },
            new DropdownSelectItem { Key = KHAN, Value = GetDisplayText(KHAN) },
            new DropdownSelectItem { Key = SROK, Value = GetDisplayText(SROK) }
        ];

        return list;
    }
}

public static class CountryAlpha3Codes
{
    public const string CAMBODIA = "KHM";
    public const string THAILAND = "THA";
    public const string VIETNAM = "VNM";
	public static string GetDisplayText(string structureType)
	{
		return structureType switch
		{
			CAMBODIA => "Cambodia",
			THAILAND => "Thailand",
			VIETNAM => "Vietnam",
			_ => ""
		};
	}

	public static List<DropdownSelectItem> GetForDropdown()
	{
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = CAMBODIA, Value = GetDisplayText(CAMBODIA) },
			new DropdownSelectItem { Key = THAILAND, Value = GetDisplayText(THAILAND) },
			new DropdownSelectItem { Key = VIETNAM, Value = GetDisplayText(VIETNAM) }
		];

		return list;
	}
}

public static class ContactPhoneChannels
{
    public const string MAIN = "MAIN";
    public const string WORK = "WORK";
    public const string PERSONAL = "PERSONAL";
    public const string CUSTOM = "CUSTOM";

	public static string GetDisplayText(string structureType)
	{
		return structureType switch
		{
			MAIN => "Main",
			WORK => "Work",
			PERSONAL => "Personal",
			CUSTOM => "Custom",
			_ => ""
		};
	}

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = MAIN, Value = GetDisplayText(MAIN) },
			new DropdownSelectItem { Key = WORK, Value = GetDisplayText(WORK) },
			new DropdownSelectItem { Key = PERSONAL, Value = GetDisplayText(PERSONAL) },
			new DropdownSelectItem { Key = CUSTOM, Value = GetDisplayText(CUSTOM) }
		];

        return list;
	}
}

public static class ContactCustomChannels
{
    public const string PERSONAL = "PERSONAL";
    public const string WORK = "WORK";
    public const string PRIMARY = "PRIMARY";
    public const string SECONDARY = "SECONDARY";
    public const string TERTIARY = "TERTIARY";
    public const string SOCIAL_MEDIA = "SOCIAL_MEDIA";
}

public static class ContactChannels
{
    public const string SMS = "SMS";
    public const string EMAIL = "EMAIL";
    public const string FACEBOOK = "FACEBOOK";
    public const string PHONE = "PHONE";
    public const string WHATSAPP = "WHATSAPP";
    public const string TELEGRAM = "TELEGRAM";
    public const string SKYPE = "SKYPE";
    public const string VIBER = "VIBER";
    public const string LINE = "LINE";

    public static string GetDesc(string channel)
    {
        return channel switch
        {
            SMS => "SMS",
            EMAIL => "e-mail",
            FACEBOOK => "Facebook",
            PHONE => "Phone",
            WHATSAPP => "Whatsapp",
            TELEGRAM => "Telegram",
            SKYPE => "Skype",
            VIBER => "Viber",
            LINE => "Line",
            _ => "",
        };
    }

    public static Dictionary<string, string> GetSocialMediaContactTypes()
    {
        Dictionary<string, string> list = new()
        {
            { FACEBOOK, GetDesc(FACEBOOK) },
            { WHATSAPP, GetDesc(WHATSAPP) },
            { TELEGRAM, GetDesc(TELEGRAM) },
            { SKYPE, GetDesc(SKYPE) },
            { VIBER, GetDesc(VIBER) },
            { LINE, GetDesc(LINE) },
        };

        return list;
    }
}

public static class CountryCallingCodes
{
    public const string CAMBODIA = "855";
    public const string THAILAND = "66";
    public const string VIETNAM = "84";

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { CAMBODIA, GetDesc(CAMBODIA) },
            { THAILAND, GetDesc(THAILAND) },
            { VIETNAM, GetDesc(VIETNAM) },

        };

        return list;
    }

    public static string GetDesc(string code)
    {
        return code switch
        {
            CAMBODIA => "855",
            THAILAND => "66",
            VIETNAM => "84",
            _ => "",
        };
    }
}

public static class CommentTypes
{
    public const string SYSTEM = "SYSTEM-MESSAGE";
    public const string SYSTEM_ASSIGNMENT = "SYSTEM-ASSIGN";
    public const string ASSIGNMENT = "ASSIGN";
    public const string GENERAL_COMMENT = "GENERAL";
    public const string OTHER = "OTHER";
    public const string FEEDBACK = "FEEDBACK";
    public const string RESOLUTION = "RESOLUTION";
    public const string ACTION = "ACTION";
    public const string ATTEMPT = "ATTEMPT";

    public static Dictionary<string, string> GetUserSelectable()
    {
        Dictionary<string, string> list = new()
        {
            { "", "" },
            { GENERAL_COMMENT, "General" },
            { FEEDBACK, "Customer Feedback" },
            { RESOLUTION, "Resolution" },
            { ACTION, "Action Taken" },
            { ATTEMPT, "Communication Attempt" }
        };
        return list;
    }
}

public static class ConfidentialityLevels
{
    public const int ADMIN = 9;
    public const int EXCO = 5;
    public const int SENIOR_MANAGEMENT = 4;
    public const int MANAGEMENT = 3;
    public const int FUNCTIONAL_EMPLOYEE = 2;
    public const int GENERAL_EMPLOYEE = 1;
    public const int PUBLIC = 0;

    public static string GetDisplayText(int confidentialityLevel)
    {
        return confidentialityLevel switch
        {
            PUBLIC => "0-Public",
            GENERAL_EMPLOYEE => "1-All Employee",
            FUNCTIONAL_EMPLOYEE => "2-Functional Employee",
            MANAGEMENT => "3-Management",
            SENIOR_MANAGEMENT => "4-Senior Management",
            EXCO => "5-Exco",
            ADMIN => "9-Administrator",
            _ => "",
        };
    }

    public static Dictionary<int, string> GetAll()
    {
        Dictionary<int, string> dict = new()
        {
            { PUBLIC, GetDisplayText(PUBLIC) },
            { GENERAL_EMPLOYEE, GetDisplayText(GENERAL_EMPLOYEE) },
            { FUNCTIONAL_EMPLOYEE, GetDisplayText(FUNCTIONAL_EMPLOYEE) },
            { MANAGEMENT, GetDisplayText(MANAGEMENT) },
            { SENIOR_MANAGEMENT, GetDisplayText(SENIOR_MANAGEMENT) },
            { EXCO, GetDisplayText(EXCO) }
        };

        return dict;
    }

    public static Dictionary<int, string> GetValidList(int userConfidentialityLevel)
    {
		Dictionary<int, string> dict = [];

        if (userConfidentialityLevel >= 0)
            dict.Add(PUBLIC, "0-Public");

        if (userConfidentialityLevel >= 1)
            dict.Add(GENERAL_EMPLOYEE, "1-All Employee");

        if (userConfidentialityLevel >= 2)
            dict.Add(FUNCTIONAL_EMPLOYEE, "2-Functional Employee");

        if (userConfidentialityLevel >= 3)
            dict.Add(MANAGEMENT, "3-Management");

        if (userConfidentialityLevel >= 4)
            dict.Add(SENIOR_MANAGEMENT, "4-Senior Management");

        if (userConfidentialityLevel >= 5)
            dict.Add(EXCO, "5-Exco");

        return dict;
    }

	public static List<DropdownSelectItem> GetForDropdown()
	{
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Id = PUBLIC, Value = GetDisplayText(PUBLIC) },
			new DropdownSelectItem { Id = GENERAL_EMPLOYEE, Value = GetDisplayText(GENERAL_EMPLOYEE) },
			new DropdownSelectItem { Id = FUNCTIONAL_EMPLOYEE, Value = GetDisplayText(FUNCTIONAL_EMPLOYEE) },
			new DropdownSelectItem { Id = MANAGEMENT, Value = GetDisplayText(MANAGEMENT) },
			new DropdownSelectItem { Id = SENIOR_MANAGEMENT, Value = GetDisplayText(SENIOR_MANAGEMENT) },
			new DropdownSelectItem { Id = EXCO, Value = GetDisplayText(EXCO) }
		];

		return list;
	}
}

public static class CommunicationTypes
{
    public const string FEEDBACK = "FEEDBACK";
    public const string REQUEST = "REQUEST";
    public const string NOTIFICATION = "NOTIFICATION";
    public const string CUSTOMER_RESPONSE = "RESPONSE";

    public static string GetDesc(string commType)
    {
        return commType switch
        {
            FEEDBACK => "Feedback",
            REQUEST => "Request",
            NOTIFICATION => "Notification",
            CUSTOMER_RESPONSE => "Customer Response",
            _ => "",
        };
    }
}

public static class DropdownDataSystems
{
    public const string SYSTEM_CORE = "SystemCore";
    public const string HEALTH_MANAGEMENT_SYSTEM = "HMS";
    public const string HUMAN_RESOURCE_MANAGEMENT = "HRM";
    public const string HOME_INVENTORY_MANAGEMENT = "HIM";
    public const string RETAIL_MANAGEMENT_SYSTEM = "RMS";
    public const string FINANCIAL_SYSTEM = "FIN";
    public const string PHARMACY_MANAGEMENT_SYSTEM = "PMS";
    public const string EVENT_MANAGEMENT_SYSTEM = "EMS";

    public static string GetNamespacePrefix(string systemCode)
    {
        return systemCode switch
        {
            FINANCIAL_SYSTEM => "DataLayer.Models.Finance",
            HEALTH_MANAGEMENT_SYSTEM => "DataLayer.Models.HMS",
            HOME_INVENTORY_MANAGEMENT => "DataLayer.Models.HomeInventory",
            PHARMACY_MANAGEMENT_SYSTEM => "DataLayer.Models.Pharmacy",
            SYSTEM_CORE => "DataLayer.Models.SysCore",
            _ => "?"
        };
    }

    public static string GetDisplayText(string? code)
    {
		return code switch
		{
			EVENT_MANAGEMENT_SYSTEM => "Event Management System (HMS)",
			HEALTH_MANAGEMENT_SYSTEM => "Health Management System (HMS)",
			HOME_INVENTORY_MANAGEMENT => "Home Inventory Management System (HIM)",
			HUMAN_RESOURCE_MANAGEMENT => "Human Resource Management System (HRM)",
            PHARMACY_MANAGEMENT_SYSTEM => "Pharmacy Management System (PMS)",
			RETAIL_MANAGEMENT_SYSTEM => "Retail Management System (RMS)",
            FINANCIAL_SYSTEM => "Financial System (FIN)",
			SYSTEM_CORE => "System Core",
			_ => "",
		};
	}

	public static List<DropdownSelectItem> GetForDropdown()
	{
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = EVENT_MANAGEMENT_SYSTEM, Value = GetDisplayText(EVENT_MANAGEMENT_SYSTEM) },
			new DropdownSelectItem { Key = HEALTH_MANAGEMENT_SYSTEM, Value = GetDisplayText(HEALTH_MANAGEMENT_SYSTEM) },
			new DropdownSelectItem { Key = HOME_INVENTORY_MANAGEMENT, Value = GetDisplayText(HOME_INVENTORY_MANAGEMENT) },
			new DropdownSelectItem { Key = HUMAN_RESOURCE_MANAGEMENT, Value = GetDisplayText(HUMAN_RESOURCE_MANAGEMENT) },
            new DropdownSelectItem { Key = PHARMACY_MANAGEMENT_SYSTEM, Value = GetDisplayText(PHARMACY_MANAGEMENT_SYSTEM) },
            new DropdownSelectItem { Key = RETAIL_MANAGEMENT_SYSTEM, Value = GetDisplayText(RETAIL_MANAGEMENT_SYSTEM) },
			new DropdownSelectItem { Key = FINANCIAL_SYSTEM, Value = GetDisplayText(FINANCIAL_SYSTEM) },
			new DropdownSelectItem { Key = SYSTEM_CORE, Value = GetDisplayText(SYSTEM_CORE) }
		];

		return list;
	}
}

public static class EducationDegree
{
    public const string BACHELOR = "BACHELOR";
    public const string MASTER = "MASTER";
    public const string HIGH_SCHOOL = "HIGH-SCHOOL";

    public static string GetDisplayText(string commType)
    {
        return commType switch
        {
            HIGH_SCHOOL => "High School",
            BACHELOR => "Bachelor",
            MASTER => "Master",
            _ => "",
        };
    }
}

public static class EmployeeContractTypes
{
    public const string PERMANENT = "PERMANENT";
    public const string CONTRACT = "CONTRACT";
    public const string INTERN = "INTERN";
    public const string TEMPORARY = "TEMPORARY";

	public static string GetDisplayText(string? entityType)
	{
		return entityType switch
		{
			PERMANENT => "Permanent",
			CONTRACT => "Contract",
			INTERN => "Intern",
			TEMPORARY => "Temporary",
			_ => " - ",
		};
	}

	public static List<DropdownSelectItem> GetForDropdown()
	{
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = PERMANENT, Value = GetDisplayText(PERMANENT) },
			new DropdownSelectItem { Key = CONTRACT, Value = GetDisplayText(CONTRACT) },
			new DropdownSelectItem { Key = INTERN, Value = GetDisplayText(INTERN) },
			new DropdownSelectItem { Key = TEMPORARY, Value = GetDisplayText(TEMPORARY) }
		];

		return list;
	}
}

public static class EmployeeStatuses
{
    public const string DRAFT = "DRAFT";
    public const string ACTIVE = "ACTIVE";
    public const string RESIGNED = "RESIGNED";
    public const string TERMINATED = "TERMINATED";
    public const string SUSPENDED = "SUSPENDED";
    public const string BLACKLIST = "BLACKLIST";
    public const string INVALID = "INVALID";

    public static bool IsValid(string employeeStatus)
    {
        return employeeStatus switch
        {
            DRAFT or ACTIVE or RESIGNED or TERMINATED or BLACKLIST or SUSPENDED or INVALID => true,
            _ => false,
        };
    }

    public static string GetDisplayText(string? entityType)
	{
		return entityType switch
		{
            DRAFT => "Draft",
			ACTIVE => "Active",
			RESIGNED => "Resigned",
			TERMINATED => "Terminated",
			SUSPENDED => "Suspended",
			BLACKLIST => "Blacklisted",
            INVALID => "Invalid",
			_ => " - ",
		};
	}

	public static List<DropdownSelectItem> GetForDropdown()
	{
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = ACTIVE, Value = GetDisplayText(ACTIVE) },
            new DropdownSelectItem { Key = DRAFT, Value = GetDisplayText(DRAFT) },
            new DropdownSelectItem { Key = RESIGNED, Value = GetDisplayText(RESIGNED) },
			new DropdownSelectItem { Key = TERMINATED, Value = GetDisplayText(TERMINATED) },
			new DropdownSelectItem { Key = SUSPENDED, Value = GetDisplayText(SUSPENDED) },
			new DropdownSelectItem { Key = BLACKLIST, Value = GetDisplayText(BLACKLIST) }
		];

		return list;
	}
}

public static class EmployeeTimeTypes
{
	public const string FULLTIME = "FULLTIME";
	public const string PARTTIME = "PARTTIME";
	

	public static string GetDisplayText(string? entityType)
	{
		return entityType switch
		{
			FULLTIME => "Full-Time",
			PARTTIME => "Part-Time",
			_ => " - ",
		};
	}

	public static List<DropdownSelectItem> GetForDropdown()
	{
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = FULLTIME, Value = GetDisplayText(FULLTIME) },
			new DropdownSelectItem { Key = PARTTIME, Value = GetDisplayText(PARTTIME) }
		];

		return list;
	}
}

public static class EntityTypes
{
    public const string COMPANY = "C";
    public const string ORGANIZATION = "O";
    public const string GROUP = "G";
    public const string BUSINESS = "B";
    public const string RESTAURANT = "R";
    public const string SHOP = "S";

    public static string GetDisplayText(string? entityType)
    {
        return entityType switch
        {
            COMPANY => "Company",
            ORGANIZATION => "Organization",
            GROUP => "Group",
            BUSINESS => "Business",
            RESTAURANT => "Restaurant",
            SHOP => "Shop",
            _ => " - ",
        };
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = BUSINESS, Value = GetDisplayText(BUSINESS) },
            new DropdownSelectItem { Key = COMPANY, Value = GetDisplayText(COMPANY) },
            new DropdownSelectItem { Key = GROUP, Value = GetDisplayText(GROUP) },
            new DropdownSelectItem { Key = ORGANIZATION, Value = GetDisplayText(ORGANIZATION) },
            new DropdownSelectItem { Key = RESTAURANT, Value = GetDisplayText(RESTAURANT) },
            new DropdownSelectItem { Key = SHOP, Value = GetDisplayText(SHOP) }
        ];

        return list;
    }
}

public static class Genders
{
    public const string MALE = "M";
    public const string FEMALE = "F";
    public const string UNKNOWN = "U";
    public const string NOT_APPLICABLE = "X";

    public static string GetDisplayText(string? gender)
    {
        return gender switch
        {
            FEMALE => "Female",
            MALE => "Male",
            NOT_APPLICABLE => "Not Applicable",
            UNKNOWN => "Unknown",
            _ => " - ",
        };
    }

    public static string GetDispalyTextKh(string? gender)
    {
        return gender switch
        {
            FEMALE => "ស្រី",
            MALE => "ប្រុស",
            NOT_APPLICABLE => "មិនមាន",
            UNKNOWN => "មិនស្គាល់",
            _ => gender.NonNullValue(" - "),
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            //{ "", "" },
            { FEMALE, GetDisplayText(FEMALE) },
            { MALE, GetDisplayText(MALE) },
            { UNKNOWN, GetDisplayText(UNKNOWN) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = MALE, Value = GetDisplayText(MALE) },
            new DropdownSelectItem { Key = FEMALE, Value = GetDisplayText(FEMALE) },
            new DropdownSelectItem { Key = UNKNOWN, Value = GetDisplayText(UNKNOWN) }
        ];

        return list;
    }
}

public static class IDDocumentTypes
{
    public const string NATIONAL_ID = "NATIONAL_ID";
    public const string PASSPORT = "PASSPORT";
    public const string BIRTH_CERTIFICATE = "BIRTH_CERTIFICATE";
    public const string FAMILY_BOOK = "FAMILY_BOOK";
    public const string OTHER = "OTHER";

    public static string GetDisplayText(string docType)
    {
        return docType switch
        {
            NATIONAL_ID => "National ID",
            PASSPORT => "Passport",
            BIRTH_CERTIFICATE => "Birth Certificate",
            FAMILY_BOOK => "Family Book",
            OTHER => "Other",
            _ => "",
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { NATIONAL_ID, GetDisplayText(NATIONAL_ID) },
            { PASSPORT, GetDisplayText(PASSPORT) },
            { BIRTH_CERTIFICATE, GetDisplayText(BIRTH_CERTIFICATE) },
        };

        return list;
    }
}

public static class ImageFileMimeTypes
{
    public const string JPEG = "image/jpeg";
    public const string PNG = "image/png";
}

public static class KeyboardKeys
{
    public const string ENTER = "ENTER";
    public const string ESCAPE = "ESCAPE";
    public const string CONTROL = "CONTROL";
    public const string ALT = "ALT";
}

public static class LanguageCodes
{
    public const string ENGLISH = "EN";
    public const string KHMER = "KH";

    public static string GetDisplayText(string languageCode)
    {
        return languageCode switch
        {
            ENGLISH => "English",
            KHMER => "Khmer",
            _ => ""
        };
    }
}

public static class MaritalStatuses
{
    public const string MARRIED = "M";
    public const string DIVORCED = "D";
    public const string SINGLE = "S";
    public const string UNKNOWN = "-";

    public static string GetDisplayText(string? maritalStatus)
    {
        return maritalStatus switch
        {
            MARRIED => "Married",
            DIVORCED => "Divorced",
            SINGLE => "Single",
            UNKNOWN => "Unknown",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { SINGLE, GetDisplayText(SINGLE) },
            { MARRIED, GetDisplayText(MARRIED) },
            { DIVORCED, GetDisplayText(DIVORCED) },
            { UNKNOWN, GetDisplayText(UNKNOWN) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = SINGLE, Value = GetDisplayText(SINGLE) },
            new DropdownSelectItem { Key = MARRIED, Value = GetDisplayText(MARRIED) },
            new DropdownSelectItem { Key = DIVORCED, Value = GetDisplayText(DIVORCED) },
            new DropdownSelectItem { Key = UNKNOWN, Value = GetDisplayText(UNKNOWN) }
        ];

        return list;
    }
}

public static class ObjectStates
{
	public const string IN_USE = "IN-USE";
	public const string WISHLIST = "WISHLIST";
	public const string LOST = "LOST";
	public const string LOANED = "LOANED";
    public const string BROKEN = "BROKEN";
    public const string WRITTEN_OFF = "WRITTEN-OFF";
    public const string DRAFT = "DRAFT";
	public const string SOLD = "SOLD";
	public const string NEW = "NEW";

    /// <summary>
    /// Brand New In-Box
    /// </summary>
	public const string BNIB = "BNIB";
    public const string GIFTED_OTHER = "GIFTED-OTHER";
    public const string RARELY_USED = "RARELY-USED";
    public const string IN_STASH = "IN-STASH";
	public const string OTHER = "OTHER";
	public const string UNKNOWN = "UNKNOWN";


	public static Dictionary<string, string> GetStateThemeClassList()
	{
		return new Dictionary<string, string>()
		{
			{ BROKEN, "status-chip-inactive-light" },
			{ WISHLIST, "status-chip-wish-light" },
			{ IN_USE, "status-chip-active-light" },
			{ LOST, "status-chip-lost-light" },
			{ BNIB, "status-chip-new-light"},
			{ LOANED, "status-chip-loan-light" },
			{ DRAFT, "status-chip-draft-light" },
			{ GIFTED_OTHER, "status-chip-fabulous-light" },
			{ RARELY_USED, "status-chip-default-light" },
			{ WRITTEN_OFF, "status-chip-default-light" },
			{ OTHER, "status-chip-default-light" },
            { UNKNOWN, "status-chip-default-light" },
			{ NEW, "status-chip-default-light" },
			{ SOLD, "status-chip-sell-light" },

		};
	}

	public static string GetDisplayText(string? state)
	{
		return state switch
		{
			BNIB => "BNIB",
			BROKEN => "Broken",
			DRAFT => "Draft",
			LOST => "Lost",
			LOANED => "Loaned",
            GIFTED_OTHER => "Gifted To Other",
			IN_STASH => "In-Stash",
			IN_USE => "In-Use",
			NEW => "New",
			RARELY_USED => "Rarely Used",
            OTHER => "Other",
			SOLD => "Sold",
			UNKNOWN => "Unknown",
			WISHLIST => "Wishlist",
            WRITTEN_OFF => "Written-Off",
			_ => "",
		};
	}

    public static Dictionary<string, string> GetLightStyles(bool isBordered=true)
    {
        return new Dictionary<string, string>()
        {
            { BROKEN, "color: #607D8B; background-color:#ECEFF1; "+(isBordered ? "border: solid 1px #607D8B;":"") },
			{ WISHLIST, "color: #9D174D; background-color:#FDF4F3; "+(isBordered ? "border: solid 1px #9D174D;":"") },
			{ IN_USE, "color: #388E3C; background-color:#E8F5E9; "+(isBordered ? "border: solid 1px #388E3C;":"") },
			{ LOST, "color: #B71C1C; background-color:#FFCDD2; "+(isBordered ? "border: solid 1px #B71C1C;":"") },
			{ BNIB, "color: #1E88E5; background-color:#E3F2FD; "+(isBordered ? "border: solid 1px #1E88E5;":"") },
			{ LOANED, "color: #FB8C00; background-color:#FFF3E0; "+(isBordered ? "border: solid 1px #FB8C00;":"") },
			{ DRAFT, "color: #8EB1CC; background-color:#E5EDF4; "+(isBordered ? "border: solid 1px #8EB1CC;":"") },
			{ GIFTED_OTHER, "color: #7E57C2; background-color:#EDE7F6; "+(isBordered ? "border: solid 1px #7E57C2;":"") },
			{ RARELY_USED, "color: #0097A7; background-color:#E0F7FA; "+(isBordered ? "border: solid 1px #0097A7;":"") },
			{ WRITTEN_OFF, "color: #4E342E; background-color:#D7CCC8; "+(isBordered ? "border: solid 1px #4E342E;":"") },
			{ OTHER, "color: #767676; background-color:#F5F5F5; "+(isBordered ? "border: solid 1px #767676;":"") },
			{ UNKNOWN, "color: #767676; background-color:#F5F5F5; "+(isBordered ? "border: solid 1px #767676;":"") },
			{ NEW, "color: #FBC02D; background-color:#FFFDE7; "+(isBordered ? "border: solid 1px #FBC02D;":"") },
			{ SOLD, "color: #9D174D; background-color:#FCE7F3; "+(isBordered ? "border: solid 1px #9D174D;":"") },

		};
	}
}

public static class ObjectStateActions
{
    public const string ADD_WISHLIST = "ADD-WISHLIST";
    public const string SAVE_DRAFT = "SAVE-DRAFT";
    public const string PURCHASE = "PURCHASE";
    public const string OWN = "OWN";
	public const string START_USING = "START-USING";
	public const string LEND_OUT = "LEND-OUT";
    public const string BROKE = "BROKE";
    public const string SELL = "SELL";
    public const string WRITE_OFF = "WRITE-OFF";
	public const string LOSE = "LOSE";
    public const string GIVEN_OTHER = "GIVEN-OTHER";
	public const string RETURN = "RETURN";
	public const string STASH = "STASH";
    public const string USE_RARELY = "USE-RARELY";

    public static Dictionary<string, string> GetActionMaterialIconList()
    {
        return new Dictionary<string, string>()
        {
			{ RETURN, "material-symbols-rounded/approval_delegation" },
			{ LOSE, "material-symbols-rounded/indeterminate_question_box" },
			{ PURCHASE, "material-symbols-rounded/shopping_cart" },
			{ BROKE, "material-symbols-rounded/watch_off" },
            { GIVEN_OTHER, "material-symbols-rounded/volunteer_activism" },
            { ADD_WISHLIST, "material-symbols-rounded/bookmark_star" },
            { SAVE_DRAFT, "material-symbols-rounded/save" },
            { SELL, "material-symbols-rounded/shopping_cart_checkout"},
            { LEND_OUT, "material-symbols-rounded/real_estate_agent" },
            { STASH, "material-symbols-rounded/warehouse" },
            { USE_RARELY, "material-symbols-rounded/waving_hand" },
			{ START_USING, "material-symbols-rounded/play_circle" },
            { WRITE_OFF, "material-symbols-rounded/power_settings_circle" }
        };
    }

    public static Dictionary<string, string> GetLightStyles(bool isBordered=true)
    {
		return new Dictionary<string, string>()
		{
			{ RETURN, "color: #1E88E5; background-color:#E3F2FD; "+(isBordered ? "border: solid 1px #1E88E5;":"") },
			{ LOSE, "color: #B71C1C; background-color:#FFCDD2; "+(isBordered ? "border: solid 1px #B71C1C;":"") },
			{ PURCHASE, "color: #1E88E5; background-color:#E3F2FD; "+(isBordered ? "border: solid 1px #1E88E5;":"") },
			{ BROKE, "color: #607D8B; background-color:#ECEFF1; "+(isBordered ? "border: solid 1px #607D8B;":"") },
			{ GIVEN_OTHER, "color: #7E57C2; background-color:#EDE7F6; "+(isBordered ? "border: solid 1px #7E57C2;":"") },
			{ ADD_WISHLIST, "color: #9D174D; background-color:#FDF4F3; "+(isBordered ? "border: solid 1px #9D174D;":"") },
			{ SAVE_DRAFT, "color: #8EB1CC; background-color:#E5EDF4; "+(isBordered ? "border: solid 1px #8EB1CC;":"") },
			{ SELL, "color: #9D174D; background-color:#FCE7F3; "+(isBordered ? "border: solid 1px #9D174D;":"") },
			{ LEND_OUT, "color: #FB8C00; background-color:#FFF3E0; "+(isBordered ? "border: solid 1px #FB8C00;":"") },
			{ STASH, "color: #8EB1CC; background-color:#E5EDF4; "+(isBordered ? "border: solid 1px #8EB1CC;":"") },
			{ USE_RARELY, "color: #0097A7; background-color:#E0F7FA; "+(isBordered ? "border: solid 1px #0097A7;":"") },
			{ START_USING, "color: #388E3C; background-color:#E8F5E9; "+(isBordered ? "border: solid 1px #388E3C;":"") },
			{ WRITE_OFF, "color: #4E342E; background-color:#D7CCC8; "+(isBordered ? "border: solid 1px #4E342E;":"") },
		};
	}

	public static string GetDisplayText(string? action)
	{
		return action switch
		{
			ADD_WISHLIST => "Add to Wishlist",
			SAVE_DRAFT => "Save as Draft",
			PURCHASE => "Purchase",
			OWN => "Own",
			LEND_OUT => "Lend Out",
            BROKE => "Broke",
            GIVEN_OTHER => "Given to Other",
			LOSE => "Lose",
			RETURN => "Return",
			STASH => "Stash",
            START_USING => "Start Using",
            WRITE_OFF => "Write-Off",
			_ => "",
		};
	}
}

public static class PersonTitles
{
    public const string MR = "MR";
    public const string MRS = "MRS";
    public const string MS = "MS";
    public const string MISS = "MISS";
    public const string DR = "DR";
    public const string EXCELLENCY = "EXCELLENCY";
    public const string DUKE = "DUKE";
    public const string DUCHESS = "DUCHESS";
    public const string OKNHA = "OKNHA";
    public const string NEAK_OKNHA = "NEAK_OKNHA";
    public const string LOK_NEAK_OKNHA = "LOK_NEAK_OKNHA";

    public static string GetDisplayText(string? personTitle)
    {
        return personTitle switch
        {
            MR => "Mr.",
            MRS => "Mrs.",
            MS => "Ms.",
            MISS => "Miss",
            DR => "Dr.",
            EXCELLENCY => "Excellency",
            DUKE => "Oknha",
            DUCHESS => "Lok Chum Teav",
            OKNHA => "Oknha",
            NEAK_OKNHA => "Neak Oknha",
            LOK_NEAK_OKNHA => "Lok Neak Oknha",
            _ => ""
        };
    }

    public static string GetDisplayTextKh(string? personTitle)
    {
        return personTitle switch
        {
            MR => "លោក",
            MRS => "លោកស្រី",
            MS => "កញ្ញា",
            MISS => "កញ្ញា",
            DR => "វេជ្ជបណ្ឌិត",
            EXCELLENCY => "ឯកឧត្តម",
            DUKE => "លោកឧកញ៉ា",
            DUCHESS => "លោកជំទាវ",
            OKNHA => "ឧកញ៉ា",
            NEAK_OKNHA => "អ្នកឧកញ៉ា",
            LOK_NEAK_OKNHA => "លោកអ្នកឧកញ៉ា",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { MR, GetDisplayText(MR) },
            { MRS, GetDisplayText(MRS) },
            { MISS, GetDisplayText(MISS) },
            { MS, GetDisplayText(MS) },
            { DR, GetDisplayText(DR) },
            { EXCELLENCY, GetDisplayText(EXCELLENCY) },
            { DUKE, GetDisplayText(DUKE) },
            { DUCHESS, GetDisplayText(DUCHESS) },
            { OKNHA, GetDisplayText(OKNHA) },
            { NEAK_OKNHA, GetDisplayText(NEAK_OKNHA) },
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = MR, Value = GetDisplayText(MR) },
            new DropdownSelectItem { Key = MRS, Value = GetDisplayText(MRS) },
            new DropdownSelectItem { Key = MISS, Value = GetDisplayText(MISS) },
            new DropdownSelectItem { Key = MS, Value = GetDisplayText(MS) },
            new DropdownSelectItem { Key = DR, Value = GetDisplayText(DR) },
            new DropdownSelectItem { Key = EXCELLENCY, Value = GetDisplayText(EXCELLENCY) },
            new DropdownSelectItem { Key = DUKE, Value = GetDisplayText(DUKE) },
            new DropdownSelectItem { Key = DUCHESS, Value = GetDisplayText(DUCHESS) }
        ];

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdownKh()
    {
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = MR, Value = "លោក" },
            new DropdownSelectItem { Key = MRS, Value = "លោកស្រី" },
            new DropdownSelectItem { Key = MISS, Value = "កញ្ញា" },
            new DropdownSelectItem { Key = MS, Value = "កញ្ញា" },
            new DropdownSelectItem { Key = DR, Value = "វេជ្ជបណ្ឌិត" },
            new DropdownSelectItem { Key = EXCELLENCY, Value = "ឯកឧត្តម" },
            new DropdownSelectItem { Key = DUKE, Value = "ឧកញ៉ា" },
            new DropdownSelectItem { Key = DUCHESS, Value = "លោកជំទាវ" }
        ];

        return list;
    }
}

public static class OrganizationStructureTypes
{
    public const string COMPANY = "COMPANY";
    public const string DEPARTMENT = "DEPARTMENT";
    public const string FUNCTION = "FUNCTION";
    public const string TEAM = "TEAM";
    public const string SUB_TEAM = "SUB-TEAM";

	public static string GetDisplayText(string? orgStructType)
	{
		return orgStructType switch
		{
			COMPANY => "Company",
			DEPARTMENT => "Department",
			FUNCTION => "Function",
			TEAM => "Team",
            SUB_TEAM => "Sub-Team",
			_ => ""
		};
	}

	public static List<DropdownSelectItem> GetForDropdown()
	{
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = COMPANY, Value = GetDisplayText(COMPANY) },
			new DropdownSelectItem { Key = DEPARTMENT, Value = GetDisplayText(DEPARTMENT) },
			new DropdownSelectItem { Key = FUNCTION, Value = GetDisplayText(FUNCTION) },
			new DropdownSelectItem { Key = TEAM, Value = GetDisplayText(TEAM) },
			new DropdownSelectItem { Key = SUB_TEAM, Value = GetDisplayText(SUB_TEAM) }
		];

		return list;
	}
}

public static class PhoneChannels
{
    public const string HOME = "HOME";
    public const string WORK = "WORK";
    public const string MOBILE = "MOBILE";
    public const string CUSTOM = "CUSTOM";

    public static string GetDisplayText(string? maritalStatus)
    {
        return maritalStatus switch
        {
            MOBILE => "Mobile",
            HOME => "Home",
            WORK => "Work",
            CUSTOM => "Custom",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { MOBILE, GetDisplayText(MOBILE) },
            { HOME, GetDisplayText(HOME) },
            { WORK, GetDisplayText(WORK) },
            { CUSTOM, GetDisplayText(CUSTOM) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = MOBILE, Value = GetDisplayText(MOBILE) },
            new DropdownSelectItem { Key = HOME, Value = GetDisplayText(HOME) },
            new DropdownSelectItem { Key = WORK, Value = GetDisplayText(WORK) },
            new DropdownSelectItem { Key = CUSTOM, Value = GetDisplayText(CUSTOM) }
        ];

        return list;
    }
}

public static class Relationships
{
    public const string FAMILY = "FAMILY";
    public const string FRIEND = "FRIEND";
    public const string RELATIVE = "RELATIVE";
    public const string COUSIN = "COUSIN";
    public const string COLLEAGUE = "COLLEAGUE";
    public const string ACQUAINTANCE = "ACQUAINTANCE";
    public const string IN_LAW = "IN-LAW";
    public const string SIBLING = "SIBLING";
    public const string BROTHER = "BROTHER";
    public const string SISTER = "SISTER";
    public const string PARENT = "PARENT";
    public const string FATHER = "FATHER";
    public const string MOTHER = "MOTHER";
    public const string CO_WORKER = "CO_WORKER";
    public const string OTHER = "OTHER";

    public static string GetDisplayText(string? relationship)
    {
        return relationship switch
        {
            ACQUAINTANCE => "Acquaitance",
            BROTHER => "Brother",
            CO_WORKER => "Co-Worker",
            COLLEAGUE => "Colleague",
            COUSIN => "Cousin",
            FAMILY => "Family",
            FATHER => "Father",
            FRIEND => "Friend",
            IN_LAW => "In-Law",
            MOTHER => "Mother",
            OTHER => "Other",
            PARENT => "Parent",
            RELATIVE => "Relative",
            SIBLING => "Sibling",
            SISTER => "Sister",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { ACQUAINTANCE, GetDisplayText(ACQUAINTANCE) },
            { BROTHER, GetDisplayText(BROTHER) },
            { CO_WORKER, GetDisplayText(CO_WORKER) },
            { COLLEAGUE, GetDisplayText(COLLEAGUE) },
            { COUSIN, GetDisplayText(COUSIN) },
            { FAMILY, GetDisplayText(FAMILY) },
            { FRIEND, GetDisplayText(FRIEND) },
            { IN_LAW, GetDisplayText(IN_LAW) },
            { MOTHER, GetDisplayText(MOTHER) },
            { OTHER, GetDisplayText(OTHER) },
            { PARENT, GetDisplayText(PARENT) },
            { RELATIVE, GetDisplayText(RELATIVE) },
            { SIBLING, GetDisplayText(SIBLING) },
            { SISTER, GetDisplayText(SISTER) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = ACQUAINTANCE, Value = GetDisplayText(ACQUAINTANCE) },
            new DropdownSelectItem { Key = BROTHER, Value = GetDisplayText(BROTHER) },
            new DropdownSelectItem { Key = CO_WORKER, Value = GetDisplayText(CO_WORKER) },
            new DropdownSelectItem { Key = COLLEAGUE, Value = GetDisplayText(COLLEAGUE) },
            new DropdownSelectItem { Key = COUSIN, Value = GetDisplayText(COUSIN) },
            new DropdownSelectItem { Key = FAMILY, Value = GetDisplayText(FAMILY) },
            new DropdownSelectItem { Key = FRIEND, Value = GetDisplayText(FRIEND) },
            new DropdownSelectItem { Key = IN_LAW, Value = GetDisplayText(IN_LAW) },
            new DropdownSelectItem { Key = MOTHER, Value = GetDisplayText(MOTHER) },
            new DropdownSelectItem { Key = OTHER, Value = GetDisplayText(OTHER) },
            new DropdownSelectItem { Key = PARENT, Value = GetDisplayText(PARENT) },
            new DropdownSelectItem { Key = RELATIVE, Value = GetDisplayText(RELATIVE) },
            new DropdownSelectItem { Key = SIBLING, Value = GetDisplayText(SIBLING) },
            new DropdownSelectItem { Key = SISTER, Value = GetDisplayText(SISTER) }
        ];

        return list;
    }
}

public static class ResponseStatusCodes
{
    public const string OK = "OK";
    public const string SUCCESS = "SUCCESS";
    public const string FAILED = "FAILED";
    public const string ERROR = "ERROR";
}

public static class RequestActions
{
    public const string CREATE = "CREATE";
    public const string CREATE_CASCADE = "CREATE-WITH-CASCADE";
    public const string READ = "READ";
    public const string UPDATE = "UPDATE";
    public const string UPDATE_CASCADE = "UPDATE-WITH-CASCADE";
    public const string DELETE = "DELETE";
    public const string HARD_DELETE = "HARD-DELETE";
    public const string TRANSIT_WORKFLOW = "TRANSIT-WORKFLOW";
    public const string SAVE_AND_TRANSIT = "SAVE-AND-TRANSIT-WORKFLOW";
}

public static class RoleCodes
{
    public const string RECEIPT_CASHIER = "RECEIPT_CASHIER";
}

public static class SystemIntervals
{
    public const string DAILY = "D";
    public const string MONTHLY = "M";
    public const string YEARLY = "Y";
    public const string QUARTERLY = "Q";
    public const string NONE = "X";
}

public static class SystemLocalizationCultures
{
    /// <summary>
    /// Khmer - Cambodia
    /// </summary>
    public const string KHMER = "km-KH";
    /// <summary>
    /// English - United State
    /// </summary>
    public const string ENGLISH_US = "en-US";
    /// <summary>
    /// English - United Kingdom
    /// </summary>
    public const string ENGLISH_GB = "en-GB";
}

public static class UserStatuses
{
	public const string ACTIVE = "ACTIVE";
	public const string RESIGNED = "RESIGNED";
	public const string TERMINATED = "TERMINATED";
	public const string SUSPENDED = "SUSPENDED";
	public const string BLACKLISTED = "BLACKLISTED";

	public static string GetDisplayText(string? entityType)
	{
		return entityType switch
		{
			ACTIVE => "Active",
			RESIGNED => "Resigned",
			TERMINATED => "Terminated",
			SUSPENDED => "Suspended",
			BLACKLISTED => "Blacklisted",
			_ => " - ",
		};
	}

	public static List<DropdownSelectItem> GetForDropdown()
	{
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem { Key = ACTIVE, Value = GetDisplayText(ACTIVE) },
			new DropdownSelectItem { Key = RESIGNED, Value = GetDisplayText(RESIGNED) },
			new DropdownSelectItem { Key = TERMINATED, Value = GetDisplayText(TERMINATED) },
			new DropdownSelectItem { Key = SUSPENDED, Value = GetDisplayText(SUSPENDED) },
			new DropdownSelectItem { Key = BLACKLISTED, Value = GetDisplayText(BLACKLISTED) }
		];

		return list;
	}
}

public static class UserTypes
{
    public const string EMPLOYEE = "EMPLOYEE";
    public const string CASHIER = "CASHIER";
    public const string CUSTOMER = "CUSTOMER";
    public const string GENERAL = "GENERAL";
	public const string PUBLIC = "PUBLIC";
	public const string STAFF = "STAFF";

    public static string GetDisplayText(string userType)
    {
        return userType switch
        {
            EMPLOYEE => "Employee",
			GENERAL => "General",
			STAFF => "Staff",
            CASHIER => "Cashier",
            CUSTOMER => "Customer",
            PUBLIC => "Public",
            _ => userType,
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> dictionary = new()
        {
            { "", "" },
			{ GENERAL, GetDisplayText(GENERAL) },
			{ EMPLOYEE, GetDisplayText(EMPLOYEE) },
            { STAFF, GetDisplayText(STAFF) },
            { CASHIER, GetDisplayText(CASHIER) },
            { CUSTOMER, GetDisplayText(CUSTOMER) },
			{ PUBLIC, GetDisplayText(PUBLIC) }
		};

        Dictionary<string, string> list = dictionary;

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem() { Key = GENERAL, Value = GetDisplayText(GENERAL) },
			new DropdownSelectItem() { Key = EMPLOYEE, Value = GetDisplayText(EMPLOYEE) },
			new DropdownSelectItem() { Key = CASHIER, Value = GetDisplayText(CASHIER) },
			new DropdownSelectItem() { Key = CUSTOMER, Value = GetDisplayText(CUSTOMER) },
			new DropdownSelectItem() { Key = PUBLIC, Value = GetDisplayText(PUBLIC) },
			new DropdownSelectItem() { Key = STAFF, Value = GetDisplayText(STAFF) }
        ];
        return list;
    }
}

/// <summary>
/// WORKFLOW ACTIONS
/// </summary>
public static class WorkflowActions
{
    public const string APPROVE = "APPROVE";
    public const string ASSIGN = "ASSIGN";
    public const string CANCEL = "CANCEL";
    public const string COMPLETE = "COMPLETE";
    public const string CONFIRM = "CONFIRM";
    public const string DROP = "DROP";
    public const string ESCALATE = "ESCALATE";
    public const string QUERY = "QUERY";
    public const string CLOSE = "CLOSE";
    public const string CREATE = "CREATE";
    
    /// <summary>
    /// System: CRM
    /// When communication status assigned|in-progress|on-hold, 
    /// Action introduce to move Communication.WorkflowStatus='ASSIGNED' |
    ///                          Communication.WorkflowStatus='IN-PROGRESS' | 
    ///                          Communication.WorkflowStatus='ON-HOLD' | 
    ///                          > Communication.WorkflowStatus='ASSIGNED'
    /// Action introduce ...
    /// </summary>
    public const string FORWARD = "FORWARD";
    public const string HOLD = "HOLD";
    public const string ISSUE = "ISSUE";
    public const string PAY = "PAY";
    public const string REJECT = "REJECT";
    /// <summary>
    /// System: CRM
    /// When communication status COMPLETE and manager want to correct communication additional information for inbound
    /// Action introduce to move Communication.WorkflowStatus='COMPLETE' > Communication.WorkflowStatus='RE-OPEN'
    /// </summary>
    public const string RE_OPEN = "RE-OPEN";
    /// <summary>
    /// System: LMS
    /// When lead is washed and updated back to system 
    /// Action introduce to move Lead.WorkflowStatus='FILTERING' > Lead.WorkflowStatus='FILTERED'
    /// </summary>
    public const string READY = "READY";
    public const string REGISTER = "REGISTER";
    public const string RESUME = "RESUME";
    /// <summary>
    /// This is embedded WorkflowAction that auto-trigger to self-assign when user pick task from pool.
    /// </summary>
    public const string SELF_PICKUP = "SELF-PICKUP";
    /// <summary>
    /// System: LMS
    /// When registered lead need to be sent for washing by Contact Center this action will invoke
    /// Action introduce to move Lead.WorkflowStatus='REGISTERED' > Lead.WorkflowStatus='FILTERING'
    /// </summary>
    public const string START_FILTER = "START-FILTER";
    public const string START_REWORK = "START-REWORK";
    public const string START_WORK = "START_WORK";
    public const string SUBMIT = "SUBMIT";
    public const string SUBMIT_AND_COMPLETE = "SUBMIT-AND-COMPLETE";
    public const string SAVE_DRAFT = "SAVE-DRAFT";
    public const string SAVE_AS_DRAFT = "SAVE-AS-DRAFT";
    public const string SUBMIT_FOR_APPROVAL = "SUBMIT-FOR-APPROVAL";
    public const string SUBMIT_AND_APRPOVE = "SUBMIT-AND-APPROVE";
    public const string TRANSFER = "TRANSFER";
    public const string VOID = "VOID";
    public const string START_INVITATION = "START-INVITATION";
    public const string CLOSE_INVITATION = "CLOSE-INVITATION";
    public const string START_REGISTRATION = "START-REGISTRATION";
    public const string HOLD_REGISTRATION = "HOLD-REGISTRATION";
    public const string RESUME_REGISTRATION = "RESUME-REGISTRATION";
    public const string END_REGISTRATION = "END-REGISTRATION";


    public static string GetDisplayText(string? workflowAction)
    {
        return workflowAction switch
        {
            ASSIGN => "Assign",
            CANCEL => "Cancel",
            CLOSE => "Close",
            COMPLETE => "Complete",
            CREATE => "Create",
            DROP => "Drop",
            HOLD => "Hold",
            READY => "Ready",
            REJECT => "Reject",
            RESUME => "Resume",
            RE_OPEN => "Re-Open",
            REGISTER => "Register",
            SAVE_DRAFT => "Save As Draft",
            SAVE_AS_DRAFT => "Save As Draft",
            SELF_PICKUP => "Assigned To Me",
            START_FILTER => "Send To Filter",
            START_WORK => "Start Work", 
            START_REWORK => "Start Rework",
            TRANSFER => "Transfer",
            FORWARD => "Forward",
            START_INVITATION => "Start Invitation",
            CLOSE_INVITATION => "Close Invitation",
            START_REGISTRATION => "Start Registration",
            SUBMIT_FOR_APPROVAL => "Submit For Approval",
            HOLD_REGISTRATION => "Hold Registration",
            END_REGISTRATION => "End Registration",
            _ => "",
        };
    }

    public static bool IsValidWorkflowAction(string workflowAction)
    {
        return workflowAction switch
        {
            REGISTER or CREATE or START_WORK or TRANSFER or CLOSE or CANCEL or ASSIGN or RESUME or START_FILTER or DROP or READY or HOLD => true,
            _ => false,
        };
    }
}

public static class WorkflowStatuses
{
    /// <summary>
    /// Always the first status
    /// </summary>
    public const string START = "START";
    public const string ASSIGNED = "ASSIGNED";
    public const string CANCELLED = "CANCELLED";
    public const string COMPLETE = "COMPLETE";
    public const string CONFIRMED = "CONFIRMED";
    public const string DRAFT = "DRAFT";
    public const string ON_HOLD = "ON-HOLD";
    public const string IN_PROGRESS = "IN-PROGRESS";
    public const string REGISTERED = "REGISTERED";
    //public const string NEW = "NEW";
    public const string CLOSE = "CLOSED";
    public const string ISSUED = "ISSUED";
    public const string PENDING_APPROVAL = "PENDING-APPROVAL";
    public const string PENDING_REVISION = "PENDING-REVISION";
    public const string RE_OPENED = "RE-OPENED";
    public const string REWORK_INPROGRESS = "REWORK-INPROGRESS";
    public const string APPROVED = "APPROVED";
    public const string PAID = "PAID";
    public const string RECONCILED = "RECONCILED";
    public const string REJECTED = "REJECTED";
    public const string INVITATION_OPEN = "INVITATION-OPEN";
    public const string INVITATION_CLOSE = "INVITATION-CLOSE";
    public const string REGISTRATION_OPEN = "REGISTRATION-OPEN";
    public const string REGISTRATION_HOLD = "REGISTRATION-HOLD";
    public const string REGISTRATION_CLOSED = "REGISTRATION-CLOSED";
    public const string VOIDED = "VOIDED";

    /// <summary>
    /// System: LMS. Status introduced in LMS/ReferrerApp to indicate that lead has has been sent and pending washing by contact center.
    /// This introduced because of Contact Center initiative that some type of lead instead of being assigned to agent right away, it would go to 
    /// washing process by contact center first to warm the lead and get more info for agent. The info may help to use in assignment of agent.
    /// 
    /// 
    /// START -> REGISTERED -> FILTERING
    /// </summary>
    public const string PENDING_FILTERING = "FILTERING";

    /// <summary>
    /// System: LMS. Status introduced in LMS/ReferrerApp to indicate that lead has has been sent and pending washing by contact center.
    /// This introduced because of Contact Center initiative that some type of lead instead of being assigned to agent right away, it would go to 
    /// washing process by contact center first to warm the lead and get more info for agent. The info may help to use in assignment of agent.
    /// 
    /// Lead goes to this status if after washing process by contact center and the decision is to drop and not proceed to pass to agent for conversion.
    /// START -> REGISTERED -> FILTERING -> DROPPED
    /// </summary>
    public const string DROPPED = "DROPPED";

    /// <summary>
    /// System: LMS. Status introduced in LMS/ReferrerApp to indicate that lead has has been sent and pending washing by contact center.
    /// This introduced because of Contact Center initiative that some type of lead instead of being assigned to agent right away, it would go to 
    /// washing process by contact center first to warm the lead and get more info for agent.
    /// 
    /// Lead goes to this status if after washing process by contact center lead is deemed ready to be passed to agent.
    /// START -> REGISTERED -> FILTERING -> FILTERED i.e. Ready for assignment
    /// </summary>
    public const string FILTERED = "FILTERED";

    public static string GetDisplayText(string? workflowStatus)
    {
        return workflowStatus switch
        {
            START => "Start",
            REGISTERED => "Registered",
            CANCELLED => "Cancelled",
            APPROVED => "Approved",
            COMPLETE => "Complete",
            DRAFT => "Draft",
            PENDING_APPROVAL => "Pending Approval",
            ON_HOLD => "On-Hold",
            INVITATION_OPEN => "Invitation Open",
            INVITATION_CLOSE => "Invitation Closed",
            REGISTRATION_OPEN => "Registration Open",
            REGISTRATION_HOLD => "Registration Held",
            REGISTRATION_CLOSED => "Registration Closed",
            _ => ""
        };
    }
}

public static class WorkflowController
{
    public static Dictionary<string, string> GetValidNextActions(string workflowObjectName, string currentWorkflowStatus, bool isManager = false)
    {
        Dictionary<string, string> list = [];

        if (workflowObjectName == "InventoryCheckIn")
        {
            switch (currentWorkflowStatus)
            {
                case WorkflowStatuses.START:
                    {
                        list.Add(WorkflowActions.SUBMIT, WorkflowActions.GetDisplayText(WorkflowActions.SUBMIT));
                    }
                    break;
                case WorkflowStatuses.REGISTERED:
                    {
                        list.Add(WorkflowActions.ASSIGN, WorkflowActions.GetDisplayText(WorkflowActions.ASSIGN));
                        list.Add(WorkflowActions.CANCEL, WorkflowActions.GetDisplayText(WorkflowActions.CANCEL));
                    }
                    break;
                case WorkflowStatuses.ASSIGNED:
                    {
                        list.Add(WorkflowActions.START_WORK, WorkflowActions.GetDisplayText(WorkflowActions.START_WORK));

                        if (isManager)
                            list.Add(WorkflowActions.TRANSFER, WorkflowActions.GetDisplayText(WorkflowActions.TRANSFER));

                        list.Add(WorkflowActions.CANCEL, WorkflowActions.GetDisplayText(WorkflowActions.CANCEL));
                    }
                    break;
                case WorkflowStatuses.IN_PROGRESS:
                    {
                        list.Add(WorkflowActions.HOLD, WorkflowActions.GetDisplayText(WorkflowActions.HOLD));
                        list.Add(WorkflowActions.CLOSE, WorkflowActions.GetDisplayText(WorkflowActions.CLOSE));
                        list.Add(WorkflowActions.CANCEL, WorkflowActions.GetDisplayText(WorkflowActions.CANCEL));
                    }
                    break;
                case WorkflowStatuses.ON_HOLD:
                    {
                        list.Add(WorkflowActions.RESUME, WorkflowActions.GetDisplayText(WorkflowActions.RESUME));
                        list.Add(WorkflowActions.CANCEL, WorkflowActions.GetDisplayText(WorkflowActions.CANCEL));
                    }
                    break;
            }
        }
        return list;
    }

    public static string GetResultingWorkflowStatus(string workflowAction)
    {
        return workflowAction switch
        {
            WorkflowActions.SUBMIT => WorkflowStatuses.PENDING_FILTERING,
            WorkflowActions.CREATE => WorkflowStatuses.REGISTERED,
            WorkflowActions.REGISTER => WorkflowStatuses.REGISTERED,
            WorkflowActions.START_WORK => WorkflowStatuses.IN_PROGRESS,
            WorkflowActions.HOLD => WorkflowStatuses.ON_HOLD,
            WorkflowActions.RESUME => WorkflowStatuses.IN_PROGRESS,
            WorkflowActions.TRANSFER => WorkflowStatuses.ASSIGNED,
            WorkflowActions.CLOSE => WorkflowStatuses.COMPLETE,
            WorkflowActions.CANCEL => WorkflowStatuses.CANCELLED,
            WorkflowActions.ASSIGN => WorkflowStatuses.ASSIGNED,
            WorkflowActions.START_FILTER => WorkflowStatuses.PENDING_FILTERING,
            WorkflowActions.READY => WorkflowStatuses.FILTERED,
            WorkflowActions.DROP => WorkflowStatuses.DROPPED,
            WorkflowActions.SELF_PICKUP => WorkflowStatuses.ASSIGNED,
            WorkflowActions.FORWARD => WorkflowStatuses.ASSIGNED,
            WorkflowActions.RE_OPEN => WorkflowStatuses.RE_OPENED,
            WorkflowActions.START_REWORK => WorkflowStatuses.REWORK_INPROGRESS,
            _ => "",
        };
    }
}

public static class UnitOfMeasuresTime
{
    public const string MINUTE = "MINUTE";
    public const string HOUR = "HOUR";
    public const string SECOND = "SECOND";
    public const string MILLISECOND = "MILLISECOND";
    public const string DAY = "DAY";
    public const string MONTH = "MONTH";
    public const string YEAR = "YEAR";
    public const string WEEK = "WEEK";
    public const string DECADE = "DECADE";
    public const string CENTURY = "CENTURY";

    public static string GetSymbol(string uom)
    {
        return uom switch
        {
            MINUTE => "min",
            HOUR => "hr",
            SECOND => "sec",
            MILLISECOND => "ms",
            DAY => "dau",
            MONTH => "mth",
            YEAR => "yr",
            WEEK => "wk",
            DECADE => "dec",
            CENTURY => "cen",
            _ => "-"
        };
    }

    public static string GetDisplayText(string uom)
    {
        return uom switch
        {
            SECOND => "Second",
            MINUTE => "Minute",
            HOUR => "Hour",
            DAY => "Day",
            WEEK => "Week",
            MONTH => "Month",
            YEAR => "Year",
            DECADE => "Decade",
            CENTURY => "Century",
            _ => uom,
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> dictionary = new()
        {
            { "", "" },
            { SECOND, GetDisplayText(SECOND) },
            { MINUTE, GetDisplayText(MINUTE) },
            { HOUR, GetDisplayText(HOUR) },
            { DAY, GetDisplayText(DAY) },
            { WEEK, GetDisplayText(WEEK) },
            { MONTH, GetDisplayText(MONTH) },
            { YEAR, GetDisplayText(YEAR) },
            { DECADE, GetDisplayText(DECADE) },
            { CENTURY, GetDisplayText(CENTURY) },
        };
        Dictionary<string, string> list = dictionary;

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem() { Key = SECOND, Value = GetDisplayText(SECOND) },
            new DropdownSelectItem() { Key = MINUTE, Value = GetDisplayText(MINUTE) },
            new DropdownSelectItem() { Key = HOUR, Value = GetDisplayText(HOUR) },
            new DropdownSelectItem() { Key = DAY, Value = GetDisplayText(DAY) },
            new DropdownSelectItem() { Key = WEEK, Value = GetDisplayText(WEEK) },
            new DropdownSelectItem() { Key = MONTH, Value = GetDisplayText(MONTH) },
            new DropdownSelectItem() { Key = YEAR, Value = GetDisplayText(YEAR) },
            new DropdownSelectItem() { Key = DECADE, Value = GetDisplayText(DECADE) },
            new DropdownSelectItem() { Key = CENTURY, Value = GetDisplayText(CENTURY) }
        ];
        return list;
    }
}

public static class UnitOfMeasuresLength
{
    public const string MILLIMETER = "MILLIMETER";
    public const string CENTIMETER = "CENTIMETER";
    public const string DECIMETER = "DECIMETER";
    public const string METER = "METER";
    public const string KILOMETER = "KILOMETER";
    public const string MILE = "MILE";

    public static string GetSymbol(string uom)
    {
        return uom switch
        {
            MILLIMETER => "mm",
            CENTIMETER => "cm",
            DECIMETER => "dm",
            METER => "m",
            KILOMETER => "km",
            MILE => "mi",
            _ => uom
        };
    }

    public static string GetDisplayText(string uom)
    {
        return uom switch
        {
            MILLIMETER => "Millimeter",
            CENTIMETER => "Centimeter",
            DECIMETER => "Decimeter",
            METER => "Meter",
            KILOMETER => "Kilometer",
            MILE => "Mile",
            _ => uom,
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> dictionary = new()
        {
            { "", "" },
            { MILLIMETER, GetDisplayText(MILLIMETER) },
            { CENTIMETER, GetDisplayText(CENTIMETER) },
            { DECIMETER, GetDisplayText(DECIMETER) },
            { METER, GetDisplayText(METER) },
            { KILOMETER, GetDisplayText(KILOMETER) },
            { MILE, GetDisplayText(MILE) }
        };
        Dictionary<string, string> list = dictionary;

        return list;
    }
}

public static class UnitOfMeasuresMass
{
    public const string MILLIGRAM = "MILLIGRAM";
    public const string CENTIGRAM = "CENTIGRAM";
    public const string DECIGRAM = "DECIGRAM";
    public const string GRAM = "GRAM";
    public const string KILOGRAM = "KILOGRAM";
    public const string TON = "TON";

    public static string GetSymbol(string uom)
    {
        return uom switch
        {
            MILLIGRAM => "mg",
            CENTIGRAM => "cg",
            DECIGRAM => "dg",
            GRAM => "g",
            TON => "t",
            _ => uom
        };
    }

    public static string GetDisplayText(string uom)
    {
        return uom switch
        {
            MILLIGRAM => "Milligram",
            CENTIGRAM => "Centigram",
            DECIGRAM => "Decigram",
            GRAM => "Gram",
            KILOGRAM => "Kilogram",
            TON => "Ton",
            _ => uom,
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> dictionary = new()
            {
                { "", "" },
                { MILLIGRAM, GetDisplayText(MILLIGRAM) },
                { CENTIGRAM, GetDisplayText(CENTIGRAM) },
                { DECIGRAM, GetDisplayText(DECIGRAM) },
                { GRAM, GetDisplayText(GRAM) },
                { KILOGRAM, GetDisplayText(KILOGRAM) },
                { TON, GetDisplayText(TON) }
            };

        Dictionary<string, string> list = dictionary;

        return list;
    }
}

public static class UnitOfMeasureTypes
{
    public const string MASS = "MASS";
    public const string MEDICINE_CONSUMABLE = "MEDICINE-CONSUMABLE";
    public const string MEDICINE_PACKAGING = "MEDICINE-PACKAGING";
    public const string VOLUME = "VOLUME";
    public const string LENGTH = "LENGTH";
    public const string POWER = "POWER";
    public const string TIME = "TIME";
    public const string VELOCITY = "VELOCITY";
    public const string RETAIL_ITEM = "RETAIL-ITEM";

    public static string GetDisplayText(string? uom)
    {
        return uom switch
        {
            MASS => "Mass",
            MEDICINE_CONSUMABLE => "Medicine Consumable",
            MEDICINE_PACKAGING => "Medicine Packing",
            VOLUME => "Volume",
            LENGTH => "Length",
            POWER => "Power",
            VELOCITY => "Velocity",
            RETAIL_ITEM => "Retail Item",
            TIME => "Time",
            _ => "",
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { "", "" },
            { MASS, GetDisplayText(MASS) },
            { MEDICINE_CONSUMABLE, GetDisplayText(MEDICINE_CONSUMABLE) },
            { MEDICINE_PACKAGING, GetDisplayText(MEDICINE_PACKAGING) },
            { VOLUME, GetDisplayText(VOLUME) },
            { LENGTH, GetDisplayText(LENGTH) },
            { POWER, GetDisplayText(POWER) },
            { RETAIL_ITEM, GetDisplayText(RETAIL_ITEM) },
            { TIME , GetDisplayText(TIME) },
            { VELOCITY, GetDisplayText(VELOCITY) },
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = LENGTH, Value = GetDisplayText(LENGTH) },
            new DropdownSelectItem { Key = MASS, Value = GetDisplayText(MASS) },
            new DropdownSelectItem { Key = MEDICINE_CONSUMABLE, Value = GetDisplayText(MEDICINE_CONSUMABLE) },
            new DropdownSelectItem { Key = MEDICINE_PACKAGING, Value = GetDisplayText(MEDICINE_PACKAGING) },
            new DropdownSelectItem { Key = POWER, Value = GetDisplayText(POWER) },
			new DropdownSelectItem { Key = RETAIL_ITEM, Value = GetDisplayText(RETAIL_ITEM) },
            new DropdownSelectItem { Key = TIME, Value = GetDisplayText(TIME) },
            new DropdownSelectItem { Key = VELOCITY, Value = GetDisplayText(VELOCITY) },
            new DropdownSelectItem { Key = VOLUME, Value = GetDisplayText(VOLUME) }
        ];

        return list;
    }
}

public enum NamingFormat
{
    /// <summary>
    /// Western Naming Format where Last Name First Name
    /// </summary>
    FirstLastNameOnly,
    /// <summary>
    /// Asian Name format where name is Surname (Middle Name if any) then Given Name
    /// </summary>
    SurnameGiveNameOnly
}