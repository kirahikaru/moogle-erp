using DataLayer.Models.SysCore;

namespace DataLayer.Models.SysCore.NonPersistent;

public class WorkflowTransitionDetail
{
    public int? ObjectId { get; set; }
    public string? ObjectType { get; set; }
    public int? TargetUserId { get; set; }
    public string? WorkflowAction { get; set; }
    public string? WorkflowActionText { get; set; }
    public string? CurrentWorkflowStatus { get; set; }
    public string? CurrentWorkflowStatusText { get; set; }
    public string? TargetWorkflowStatus { get; set; }
    public string? TargetWorkflowStatusText { get; set; }
    public string? TransitionRemark { get; set; }

    public User? TargetUser { get; set; }
}