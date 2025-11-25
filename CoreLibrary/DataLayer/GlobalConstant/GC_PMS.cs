using DataLayer.Models;

namespace DataLayer.GlobalConstant;

#region WORKFLOW CONTROLLERS
/// <summary>
/// Workflow Controller - InventoryCheckIn
/// </summary>
public static class WFC_PurchaseInvoice 
{
    public static bool IsValidWorkflowTransit(string currentWorkflowStatus, string workflowAction)
    {
        List<string> list = new()
        {
            { WorkflowStatuses.START + WorkflowActions.SAVE_AS_DRAFT },
            { WorkflowStatuses.START + WorkflowActions.REGISTER },
            { WorkflowStatuses.DRAFT + WorkflowActions.CANCEL },
            { WorkflowStatuses.DRAFT + WorkflowActions.REGISTER },
            { WorkflowStatuses.REGISTERED + WorkflowActions.CANCEL },
            { WorkflowStatuses.REGISTERED + WorkflowActions.SUBMIT_FOR_APPROVAL },
            { WorkflowStatuses.PENDING_REVISION + WorkflowActions.CANCEL },
            { WorkflowStatuses.PENDING_REVISION + WorkflowActions.SUBMIT_FOR_APPROVAL },
            { WorkflowStatuses.PENDING_APPROVAL + WorkflowActions.APPROVE },
            { WorkflowStatuses.PENDING_APPROVAL + WorkflowActions.QUERY },
            { WorkflowStatuses.PENDING_APPROVAL + WorkflowActions.REJECT },
            { WorkflowStatuses.APPROVED + WorkflowActions.ISSUE },
            { WorkflowStatuses.APPROVED + WorkflowActions.VOID },
            { WorkflowStatuses.ISSUED + WorkflowActions.PAY },
            { WorkflowStatuses.ISSUED + WorkflowActions.VOID },
        };

        return list.Contains(currentWorkflowStatus + workflowAction);
    }

    public static string GetResultingWorkflowStatus(string workflowAction)
    {
        return workflowAction switch
        {
            WorkflowActions.SAVE_AS_DRAFT => WorkflowStatuses.DRAFT,
            WorkflowActions.SUBMIT_FOR_APPROVAL => WorkflowStatuses.PENDING_APPROVAL,
            WorkflowActions.SUBMIT_AND_APRPOVE => WorkflowStatuses.APPROVED,
            WorkflowActions.CANCEL => WorkflowStatuses.CANCELLED,
            WorkflowActions.QUERY => WorkflowStatuses.PENDING_REVISION,
            WorkflowActions.APPROVE => WorkflowStatuses.APPROVED,
            WorkflowActions.REJECT => WorkflowStatuses.REJECTED,
            WorkflowActions.ISSUE => WorkflowStatuses.ISSUED,
            WorkflowActions.VOID => WorkflowStatuses.VOIDED,
            WorkflowActions.PAY => WorkflowStatuses.PAID,
            _ => ""
        };
    }

    public static Dictionary<string, string> GetNextValidWorkflowActions(string currentWorkflowStatus, bool isApprover = false)
    {
        Dictionary<string, string> list = [];

        switch (currentWorkflowStatus)
        {
            case WorkflowStatuses.START:
                {
                    list.Add(WorkflowActions.SAVE_AS_DRAFT, "Save As Draft");

                    if (isApprover)
                        list.Add(WorkflowActions.SUBMIT_FOR_APPROVAL, "Submit For Approval");
                    else
                        list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "Submit & Approve");
                }
                break;
            case WorkflowStatuses.DRAFT:
                {
                    if (isApprover)
                        list.Add(WorkflowActions.SUBMIT_FOR_APPROVAL, "Submit For Approval");
                    else
                        list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "Submit & Approve");

                    list.Add(WorkflowActions.CANCEL, "Cancel");
                }
                break;
            case WorkflowStatuses.PENDING_APPROVAL:
                {
                    list.Add(WorkflowActions.APPROVE, "Approve");
                    list.Add(WorkflowActions.QUERY, "Query");
                    list.Add(WorkflowActions.REJECT, "Reject");
                }
                break;
            case WorkflowStatuses.PENDING_REVISION:
                {
                    if (isApprover)
                        list.Add(WorkflowActions.SUBMIT_FOR_APPROVAL, "Submit For Approval");
                    else
                        list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "Submit & Approve");
                    list.Add(WorkflowActions.CANCEL, "Cancel");
                }
                break;
            case WorkflowStatuses.APPROVED:
                {
                    list.Add(WorkflowActions.ISSUE, "Issue");
                    list.Add(WorkflowActions.VOID, "Void");
                }
                break;
            case WorkflowStatuses.ISSUED:
                {
                    list.Add(WorkflowActions.PAY, "Pay");
                    list.Add(WorkflowActions.VOID, "Void");
                }
                break;
            default:
                break;
        }

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
        List<DropdownSelectItem> list =
        [
            new() { Key = WorkflowStatuses.DRAFT },
            new() { Key = WorkflowStatuses.REGISTERED },
            new() { Key = WorkflowStatuses.PENDING_APPROVAL },
            new() { Key = WorkflowStatuses.PENDING_REVISION },
            new() { Key = WorkflowStatuses.APPROVED },
            new() { Key = WorkflowStatuses.REJECTED },
            new() { Key = WorkflowStatuses.CANCELLED }
        ];

        return list;
    }
}

public static class WFC_PurchaseOrder 
{
    public static bool IsValidWorkflowTransit(string currentWorkflowStatus, string workflowAction)
    {
        List<string> list = new()
        {
            { WorkflowStatuses.START + WorkflowActions.SAVE_AS_DRAFT },
            { WorkflowStatuses.START + WorkflowActions.CONFIRM },
            { WorkflowStatuses.DRAFT + WorkflowActions.CONFIRM },
            { WorkflowStatuses.DRAFT + WorkflowActions.CANCEL },
            { WorkflowStatuses.COMPLETE + WorkflowActions.CANCEL },
            { WorkflowStatuses.REGISTERED + WorkflowActions.SUBMIT_FOR_APPROVAL },
            { WorkflowStatuses.PENDING_REVISION + WorkflowActions.CANCEL },
            { WorkflowStatuses.PENDING_REVISION + WorkflowActions.SUBMIT_FOR_APPROVAL },
            { WorkflowStatuses.PENDING_APPROVAL + WorkflowActions.APPROVE },
            { WorkflowStatuses.PENDING_APPROVAL + WorkflowActions.QUERY },
            { WorkflowStatuses.PENDING_APPROVAL + WorkflowActions.REJECT },
            { WorkflowStatuses.APPROVED + WorkflowActions.ISSUE },
            { WorkflowStatuses.APPROVED + WorkflowActions.VOID },
            { WorkflowStatuses.ISSUED + WorkflowActions.PAY },
            { WorkflowStatuses.ISSUED + WorkflowActions.VOID },
        };

        return list.Contains(currentWorkflowStatus + workflowAction);
    }

    public static string GetResultingWorkflowStatus(string workflowAction)
    {
        return workflowAction switch
        {
            WorkflowActions.SAVE_AS_DRAFT => WorkflowStatuses.DRAFT,
            WorkflowActions.SUBMIT_FOR_APPROVAL => WorkflowStatuses.PENDING_APPROVAL,
            WorkflowActions.SUBMIT_AND_APRPOVE => WorkflowStatuses.APPROVED,
            WorkflowActions.CANCEL => WorkflowStatuses.CANCELLED,
            WorkflowActions.QUERY => WorkflowStatuses.PENDING_REVISION,
            WorkflowActions.APPROVE => WorkflowStatuses.APPROVED,
            WorkflowActions.REJECT => WorkflowStatuses.REJECTED,
            WorkflowActions.ISSUE => WorkflowStatuses.ISSUED,
            WorkflowActions.VOID => WorkflowStatuses.VOIDED,
            WorkflowActions.PAY => WorkflowStatuses.PAID,
            _ => ""
        };
    }

    public static Dictionary<string, string> GetNextValidWorkflowActions(string currentWorkflowStatus, bool isApprover = false)
    {
        Dictionary<string, string> list = [];

        switch (currentWorkflowStatus)
        {
            case WorkflowStatuses.START:
                {
                    list.Add(WorkflowActions.SAVE_AS_DRAFT, "Save As Draft");

                    if (isApprover)
                        list.Add(WorkflowActions.SUBMIT_FOR_APPROVAL, "Submit For Approval");
                    else
                        list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "Submit & Approve");
                }
                break;
            case WorkflowStatuses.DRAFT:
                {
                    if (isApprover)
                        list.Add(WorkflowActions.SUBMIT_FOR_APPROVAL, "Submit For Approval");
                    else
                        list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "Submit & Approve");

                    list.Add(WorkflowActions.CANCEL, "Cancel");
                }
                break;
            case WorkflowStatuses.PENDING_APPROVAL:
                {
                    list.Add(WorkflowActions.APPROVE, "Approve");
                    list.Add(WorkflowActions.QUERY, "Query");
                    list.Add(WorkflowActions.REJECT, "Reject");
                }
                break;
            case WorkflowStatuses.PENDING_REVISION:
                {
                    if (isApprover)
                        list.Add(WorkflowActions.SUBMIT_FOR_APPROVAL, "Submit For Approval");
                    else
                        list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "Submit & Approve");
                    list.Add(WorkflowActions.CANCEL, "Cancel");
                }
                break;
            case WorkflowStatuses.APPROVED:
                {
                    list.Add(WorkflowActions.ISSUE, "Issue");
                    list.Add(WorkflowActions.VOID, "Void");
                }
                break;
            case WorkflowStatuses.ISSUED:
                {
                    list.Add(WorkflowActions.PAY, "Pay");
                    list.Add(WorkflowActions.VOID, "Void");
                }
                break;
            default:
                break;
        }

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
        List<DropdownSelectItem> list =
        [
            new() { Key = WorkflowStatuses.DRAFT },
            new() { Key = WorkflowStatuses.REGISTERED },
            new() { Key = WorkflowStatuses.PENDING_APPROVAL },
            new() { Key = WorkflowStatuses.PENDING_REVISION },
            new() { Key = WorkflowStatuses.APPROVED },
            new() { Key = WorkflowStatuses.REJECTED },
            new() { Key = WorkflowStatuses.CANCELLED }
        ];

        return list;
    }
}
#endregion
