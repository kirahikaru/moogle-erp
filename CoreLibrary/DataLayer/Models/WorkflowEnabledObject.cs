using DataLayer.GlobalConstant;

namespace DataLayer.Models;

public class WorkflowEnabledObject : AuditObject
{
    public DateTime? AssignedDateTime { get; set; }
    public DateTime? ApprovedDateTime { get; set; }
    public DateTime? CompleteDateTime { get; set; }
    public int? AssignedUserId { get; set; }
    public string WorkflowStatus { get; set; }

    public WorkflowEnabledObject() : base()
    {
        this.WorkflowStatus = WorkflowStatuses.START;
    }
}