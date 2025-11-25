namespace DataLayer.Models.SysCore.NonPersistent;

public class DatabaseUpdateSuggestion
{
    public string? TableName { get; set; }
    public string? FieldName { get; set; }
    public string? UpdateScript { get; set; }
}