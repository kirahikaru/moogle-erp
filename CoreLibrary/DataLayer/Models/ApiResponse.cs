using DataLayer.GlobalConstant;

namespace DataLayer.Models;

public class ApiResponse
{
    public string? UserId { get; set; }
    public string Username { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? RequestAction { get; set; }
    public string? ObjectType { get; set; }
    public string? ObjectCode { get; set; }
    public int? ObjectId { get; set; }
    public bool IsSuccess { get; set; }
    public string? MessageCode { get; set; }
    public string? DisplayMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public ApiResponse()
    {
        Username = "";
        ObjectCode = "";
        ObjectType = "";
        IsSuccess = false;
        Timestamp = DateTime.UtcNow.AddHours(7);
        RequestAction = "";
        MessageCode = "";
        DisplayMessage = "";
        ErrorMessage = "";
    }

    public void SetSuccess(string displayMessage = "")
    {
        IsSuccess = true;
        MessageCode = ResponseStatusCodes.SUCCESS;
        DisplayMessage = displayMessage;
    }

    public void SetFailure(string displayMessage)
    {
        IsSuccess = false;
        MessageCode = ResponseStatusCodes.SUCCESS;
        DisplayMessage = displayMessage;
        ErrorMessage = "";
    }

    public void SetError(string displayMessage, string errorMessage)
    {
        IsSuccess = false;
        MessageCode = ResponseStatusCodes.ERROR;
        DisplayMessage = displayMessage;
        ErrorMessage = errorMessage;
    }
}