namespace DataLayer.AuxComponents.Extensions;

public static class IntegerExt
{
    public static string ToCurrencyText(
        this int value,
        bool displayDecimal = false,
        string? currSymbolCode = null,
        CurrencyDisplayOption displayOption = CurrencyDisplayOption.PrefixNoSpace)
    {
        StringBuilder sb = new();

        if (displayDecimal)
            sb.Append(value!.ToString("#,##0.00", CultureInfo.CurrentCulture));
        else
            sb.Append(value!.ToString("#,##0", CultureInfo.CurrentCulture));

        if (!string.IsNullOrEmpty(currSymbolCode))
        {
            switch (displayOption)
            {
                case CurrencyDisplayOption.PrefixNoSpace:
                    sb.Insert(0, currSymbolCode);
                    break;
                case CurrencyDisplayOption.PrefixWithSpace:
                    sb.Insert(0, currSymbolCode + " ");
                    break;
                case CurrencyDisplayOption.SuffixNoSpace:
                    sb.Append(currSymbolCode);
                    break;
                case CurrencyDisplayOption.SuffixWithSpace:
                    sb.Append(" " + currSymbolCode);
                    break;
                default: break;
            }
        }

        return sb.ToString();
    }

    public static string ToText(
        this int? value,
        bool addComma = false)
    {
        if (value == null)
            return string.Empty;
        else
            return value.Value.ToString(addComma ? "#,##0" : "0", CultureInfo.CurrentCulture);
	}
}