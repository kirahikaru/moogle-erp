using System.Globalization;

namespace DataLayer.AuxComponents.Extensions;

public static class IntegerExt
{
    public static string ToCurrencyText(
        this int value,
        bool displayDecimal = false,
        string? currencySymbolCode = null,
        CurrencyDisplayOption displayOption = CurrencyDisplayOption.PrefixNoSpace)
    {
        StringBuilder sb = new();

        if (displayDecimal)
            sb.Append(value!.ToString("#,##0.00", CultureInfo.CurrentCulture));
        else
            sb.Append(value!.ToString("#,##0", CultureInfo.CurrentCulture));

        if (!string.IsNullOrEmpty(currencySymbolCode))
        {
            switch (displayOption)
            {
                case CurrencyDisplayOption.PrefixNoSpace:
                    sb.Insert(0, currencySymbolCode);
                    break;
                case CurrencyDisplayOption.PrefixWithSpace:
                    sb.Insert(0, currencySymbolCode + " ");
                    break;
                case CurrencyDisplayOption.SuffixNoSpace:
                    sb.Append(currencySymbolCode);
                    break;
                case CurrencyDisplayOption.SuffixWithSpace:
                    sb.Append(" " + currencySymbolCode);
                    break;
                default: break;
            }
        }

        return sb.ToString();
    }
}