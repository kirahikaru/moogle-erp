namespace DataLayer.Models.SysCore.NonPersistent;

public class RunNumGenParam
{
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime? ActionDateTime { get; set; }
    public string? ObjectClassName { get; set; }
    public DateTime? BusinessDate { get; set; }

    public RunNumGenParam()
    {
            
    }

    public RunNumGenParam(int userId, string userName, DateTime actionDateTime, DateTime businessDate, string objectClassName)
    {
        UserId = userId;
        UserName = userName;
        ActionDateTime = actionDateTime;
        BusinessDate = businessDate;
        ObjectClassName = objectClassName;
    }
}
