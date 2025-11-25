using DataLayer.GlobalConstant;
using System.Text.RegularExpressions;

namespace DataLayer.AuxComponents.Helpers;

public static class ContactFormatHelper
{
    public static string FormatPhoneText(string ccc, string value, string countryCode)
    {
        string resultStr;

        if (String.IsNullOrEmpty(value))
            return value;

        value = value.Replace(" ", "");

        switch (ccc)
        {
            case CountryCallingCodes.CAMBODIA:
                {
                    string prefixFormat1Pattern = @"^8550[\d]{3,}";
					string prefixFormat2Pattern = @"^855[\d]{3,}";

                    if (Regex.IsMatch(value, prefixFormat1Pattern))
                        resultStr = $"{value.Substring(3, 3)} {value.Substring(6, 3)} {value[9..]}";
                    else if (Regex.IsMatch(value, prefixFormat2Pattern))
                        resultStr = $"0{value.Substring(3, 2)} {value.Substring(5, 3)} {value[8..]}";
                    else if (value[..1] != "0")
                        resultStr = $"0{value[..2]} {value.Substring(2, 3)} {value[5..]}";
                    else
                        resultStr = $"{value[..3]} {value.Substring(3, 3)} {value[6..]}";
                }
                break;
            default: return ccc.IsAtLeast(1) ? $"+{ccc} {value}" : $"{value}";
        }

        return resultStr;
    }
}