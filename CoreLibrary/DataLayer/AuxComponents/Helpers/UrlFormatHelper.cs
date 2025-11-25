using System.Text.RegularExpressions;

namespace DataLayer.AuxComponents.Helpers;

public static class UrlFormatHelper
{
    public static bool IsValidFacebookUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return false;
        else
        {
            return url.StartsWith("https://www.facebook.com/", StringComparison.InvariantCultureIgnoreCase) ||
                url.StartsWith("http://www.facebook.com/", StringComparison.InvariantCultureIgnoreCase) ||
                url.StartsWith("www.facebook.com/", StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public static bool IsValidEmailAddressFormat(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return false;
        else
        {
            Regex regex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email!);
        }
    }
}