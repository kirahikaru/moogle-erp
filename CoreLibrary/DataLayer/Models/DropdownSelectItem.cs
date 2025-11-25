namespace DataLayer.Models;

public class DropdownSelectItem
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string ValueKh { get; set; }

    public DropdownSelectItem()
    {
        Key = "";
        Value = "";
        ValueKh = "";
    }
}