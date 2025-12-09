using DataLayer.Models;

namespace DataLayer.GlobalConstant;

#region HMS - Healthcare Management System
public static class DoctorStatuses
{
    public const string DRAFT = "DRAFT";
    public const string ACTIVE = "ACTIVE";
    public const string INACTIVE = "INACTIVE";
    public const string TERMINATED = "TERMINATED";
    public const string BLACKLISTED = "BLACKLIST";

    public static bool IsValid(string customerStatus)
    {
        return customerStatus switch
        {
            DRAFT or ACTIVE or INACTIVE or TERMINATED or BLACKLISTED => true,
            _ => false,
        };
    }

    public static string GetDisplayText(string? customerStatus)
    {
        return customerStatus switch
        {
            DRAFT => "Draft",
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
            { DRAFT, GetDisplayText(DRAFT) },
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

public static class HealthcareFacilityTypes
{
    public const string HOSPITAL = "HOSPITAL";
    public const string CLINIC = "CLINIC";
    public const string POLYCLINIC = "POLYCLINIC";

    public static string GetDisplayText(string? facilityType)
    {
        return facilityType switch
        {
            HOSPITAL => "Hospital",
            CLINIC => "Clinic",
            POLYCLINIC => "Polyclinic",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { HOSPITAL, GetDisplayText(HOSPITAL) },
            { CLINIC, GetDisplayText(CLINIC) },
            { POLYCLINIC, GetDisplayText(POLYCLINIC) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = HOSPITAL, Value = GetDisplayText(HOSPITAL) },
            new DropdownSelectItem { Key = CLINIC, Value = GetDisplayText(CLINIC) },
            new DropdownSelectItem { Key = POLYCLINIC, Value = GetDisplayText(POLYCLINIC) }
        ];

        return list;
    }
}
#endregion
