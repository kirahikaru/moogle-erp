namespace DataLayer.AuxComponents.Extensions;

public static class StringExt
{
    public static bool Is(this string? value, params string[] validValues)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        foreach (string v in validValues)
        {
            if (value == v)
                return true;
        }

        return false;
    }

    public static bool Is(this string? value, List<string> validValues)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return validValues.Contains(value!);
    }

    public static bool IsNotNull(this string value)
    {
        return value != null;
    }

    public static string NonNullValue(this string? value, string dspTxtIfNullOrBlank = "", bool isWithoutSpace = false)
    {
        if (isWithoutSpace)
        {
            return (value ?? "").Replace(" ", "");
        }
        else
        {
            return string.IsNullOrEmpty(value) ? dspTxtIfNullOrBlank : value!;
        }
    }

    public static bool HasValue(this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public static bool IsAtLeast(this string? value, int strLength)
    {
        return !string.IsNullOrEmpty(value) && value.Length >= strLength;
    }
}
