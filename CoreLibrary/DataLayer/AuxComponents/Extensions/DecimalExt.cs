namespace DataLayer.AuxComponents.Extensions;

public static class DecimalExt
{
    public static bool IsNullOrZero(this decimal? value)
    {
        return value == null || value == 0;
    }

    public static string ToCurrencyText(this decimal? value, 
        bool displayDecimal = true, 
        string? currencySymbolCode = null,
        CurrencyDisplayOption displayOption = CurrencyDisplayOption.PrefixNoSpace)
    {
        StringBuilder sb = new();

        if (value == null)
            return string.Empty;

        if (displayDecimal)
			sb.Append(value!.Value.ToString("#,##0.00"));
        else
			sb.Append(value.Value.ToString("#,##0", CultureInfo.CurrentCulture));

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

    public static string ToCurrencyText(
        this decimal value, 
        bool displayDecimal = true, 
        string? currencySymbolCode = null,
        CurrencyDisplayOption displayOption = CurrencyDisplayOption.PrefixNoSpace)
    {
		StringBuilder sb = new();

		if (displayDecimal)
			sb.Append(value.ToString("#,##0.00", CultureInfo.CurrentCulture));
        else
			sb.Append(value.ToString("#,##0", CultureInfo.CurrentCulture));

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

public enum CurrencyDisplayOption
{
    PrefixNoSpace, PrefixWithSpace, SuffixNoSpace, SuffixWithSpace
}