using System.Globalization;

namespace DataLayer.AuxComponents.Extensions;

public static class DateTimeExt
{
    public static bool IsNullOrZero(this decimal? value)
    {
        return value == null || value == 0;
    }

    public static int GetAge(this DateTime? value, DateTime? ageOnDate = null)
    {
        if (value == null) return -1;

        if (ageOnDate == null)
            ageOnDate = DateTime.Now;

        //source: https://www.c-sharpcorner.com/code/961/how-to-calculate-age-from-date-of-birth-in-c-sharp.aspx
        int age = ageOnDate.Value.Year - value!.Value.Year;

        if (ageOnDate.Value.DayOfYear < value.Value.DayOfYear)
            age--;

        return age;
    }

    public static string ToShortDateString(this DateTime? value, string displayIfNull="")
    {
        if (value == null)
            return displayIfNull;
        else
            return value!.Value.ToString("dd-MMM-yyyy");
    }

    public static string ToShortDateTimeString(this DateTime? value, bool hourMode24 = true, bool showSecond = false)
    {
        if (value == null)
            return " - ";

        if (hourMode24)
        {
            if (showSecond)
                return value!.Value.ToString("dd-MMM-yyyy HH:mm:ss");
            else
                return value!.Value.ToString("dd-MMM-yyyy HH:mm");
        }
        else
        {
            if (showSecond)
                return value!.Value.ToString("dd-MMM-yyyy hh:mm:ss tt");
            else
                return value!.Value.ToString("dd-MMM-yyyy hh:mm tt");
        }
    }

    //public static int GetAgeAtLastBirthDay(this DateTime? value, DateTime? ageOnDate)
    //{

    //}
}