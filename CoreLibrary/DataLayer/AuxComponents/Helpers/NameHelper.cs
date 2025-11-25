using DataLayer.GlobalConstant;
namespace DataLayer.AuxComponents.Helpers;

public static class NameHelper
{
    public static string FormFullName(string? firstName, string? middleName, string? lastName, NamingFormat nameFormat = NamingFormat.FirstLastNameOnly)
    {
        StringBuilder sb = new();

        if (nameFormat == NamingFormat.FirstLastNameOnly)
        {
            if (!String.IsNullOrEmpty(firstName))
                sb.Append(firstName.Trim());

            if (!String.IsNullOrEmpty(middleName))
                if (sb.Length == 0)
                    sb.Append(middleName.Trim());
                else
                    sb.Append(" " + middleName.Trim());

            if (!String.IsNullOrEmpty(lastName))
                if (sb.Length == 0)
                    sb.Append(lastName.Trim());
                else
                    sb.Append(" " + lastName.Trim());
        }
        else if (nameFormat == NamingFormat.SurnameGiveNameOnly)
        {
            if (!String.IsNullOrEmpty(lastName))
                if (sb.Length == 0)
                    sb.Append(lastName.Trim());
                else
                    sb.Append(" " + lastName.Trim());

            if (!String.IsNullOrEmpty(middleName))
                if (sb.Length == 0)
                    sb.Append(middleName.Trim());
                else
                    sb.Append(" " + middleName.Trim());

            if (!String.IsNullOrEmpty(firstName))
                if (sb.Length == 0)
                    sb.Append(firstName.Trim());
                else
                    sb.Append(" " + firstName.Trim());
        }

        return sb.ToString();
    }
}