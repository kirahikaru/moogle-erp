using DataLayer.GlobalConstant;
using MongoDB.Driver;

namespace DataLayer.AuxComponents.Extensions;

public static class CurrencyExtension
{
    public static bool IsCurrencyHasDecimal(string? currencyCode)
    {
        return currencyCode switch
        {
            Currencies.CAMBODIA_KHR => false,
            Currencies.THAI_THB => false,
            Currencies.VIETNAM_VND => false,
            _ => true
        };
    }

    public static string ToDisplayText(decimal value, string currencyCode, 
        CurrencyAppendType appendCurrencySymbol = CurrencyAppendType.Prefix, 
        CurrencyDisplayUnit displayUnit = CurrencyDisplayUnit.Symbol)
    {
        StringBuilder sb = new();

        if (appendCurrencySymbol == CurrencyAppendType.Prefix)
            sb.Append(displayUnit == CurrencyDisplayUnit.Symbol ? Currencies.GetSymbol(currencyCode) : currencyCode);

        switch (currencyCode)
        {
            case Currencies.CAMBODIA_KHR:
            case Currencies.THAI_THB:
            case Currencies.VIETNAM_VND:
                sb.Append(value.ToString("#,##0"));
                break;
            default:
                sb.Append(value.ToString("#,##0.00"));
                break;
        }

        if (appendCurrencySymbol == CurrencyAppendType.Suffix)
            sb.Append(displayUnit == CurrencyDisplayUnit.Symbol ? Currencies.GetSymbol(currencyCode) : currencyCode);

        return sb.ToString();
    }

    public static decimal RoundUp(decimal value, string? currencyCode)
    {

        switch(currencyCode)
        {
            case Currencies.CAMBODIA_KHR:     // 100 currency
                {
                    decimal x = value % 100;
                    if (x > 0)
                        value = value - x + 100;
                } break;
            case Currencies.VIETNAM_VND:
            case Currencies.THAI_THB:     // 1 currency
                {
                    decimal x = value % 1;
                    if (x > 0)
                        value = value - x + 1;
                } break;
            default:
                {
                    decimal x = value % 0.01M;

                    if (x > 0)
                        value = value - x + 0.01M;
                }
                break;
        }

        return value;
    }

    public static decimal RoundDown(decimal value, string? currencyCode)
    {
        switch (currencyCode)
        {
            case "KHR":     // 100 currency
                {
                    decimal x = value % 100;
                    if (x > 0)
                        value -= x;
                }
                break;
            case "THB":     // 1 currency
                {
                    decimal x = value % 1;
                    if (x > 0)
                        value -= x;
                }
                break;
            default:
                {
                    decimal x = value % 0.01M;

                    if (x > 0)
                        value -= x;
                }
                break;
        }

        return value;
    }
}

public enum CurrencyAppendType
{
    None, Prefix, Suffix
}

public enum CurrencyDisplayUnit
{
    Symbol, IsoCurrencyCode
}