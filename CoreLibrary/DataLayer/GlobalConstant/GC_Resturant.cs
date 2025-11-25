namespace DataLayer.GlobalConstant;

public static class CategoryType
{
    public const string F = "F";
    public const string B = "B";
    public const string O = "O";
    public static string GetDisplayText(string structureType)
    {
        return structureType switch
        {
            F => "Food",
            B => "Beverage",
            O => "Other",
            _ => ""
        };
    }
    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new Dictionary<string, string>
        {
            { F, GetDisplayText(F) },
            { B, GetDisplayText(B) },
            { O, GetDisplayText(O) }
        };
        return list;
    }
}
