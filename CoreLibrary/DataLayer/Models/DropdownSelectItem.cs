namespace DataLayer.Models;

public class DropdownSelectItem
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string ValueKh { get; set; }

    [Computed, Write(false), ReadOnly(true)]
    public string ValueKeyText => Value.NonNullValue("") + (!string.IsNullOrEmpty(Key) ? $" ({Key})" : "");

    public DropdownSelectItem()
    {
        Key = "";
        Value = "";
        ValueKh = "";
    }
}