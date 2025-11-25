namespace DataLayer.Models.SysCore.NonPersistent;

public class ResponseStatus
{
    public bool IsSuccess { get; set; }
    public string? StatusCode { get; set; }
    public string? DisplayMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public ResponseStatus()
    {
		IsSuccess = false;
    }
}