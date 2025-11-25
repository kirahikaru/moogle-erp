using DataLayer.Models;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace DataLayer.GlobalConstant;

public static class ReceiptDiscountTypes
{
    public const string AMOUNT = "A";
    public const string PERCENTAGE = "P";

    public static string GetDisplayText(string? manufacturerStatus)
    {
        return manufacturerStatus switch
        {
            AMOUNT => "Fix Amount",
            PERCENTAGE => "Percentage",
            _ => ""
        };
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
            [
                new DropdownSelectItem { Key = AMOUNT, Value = GetDisplayText(AMOUNT) },
                new DropdownSelectItem { Key = PERCENTAGE, Value = GetDisplayText(PERCENTAGE) }
            ];

        return list;
    }
}

public static class ManufacturerStatuses
{
    public const string ACTIVE = "ACTIVE";
    public const string SUSPENDED = "SUSPENDED";
    public const string TERMINATED = "TERMINATED";
    public const string INACTIVE = "INACTIVE";

    public static string GetDisplayText(string? manufacturerStatus)
    {
        return manufacturerStatus switch
        {
            ACTIVE => "Active",
            SUSPENDED => "Suspended",
            TERMINATED => "Terminated",
            INACTIVE => "Inactive",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { ACTIVE, GetDisplayText(ACTIVE) },
            { SUSPENDED, GetDisplayText(SUSPENDED) },
            { TERMINATED, GetDisplayText(TERMINATED) },
            { INACTIVE, GetDisplayText(INACTIVE) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = ACTIVE, Value = GetDisplayText(ACTIVE) },
            new DropdownSelectItem { Key = SUSPENDED, Value = GetDisplayText(SUSPENDED) },
            new DropdownSelectItem { Key = TERMINATED, Value = GetDisplayText(TERMINATED) },
            new DropdownSelectItem { Key = INACTIVE, Value = GetDisplayText(INACTIVE) }
        ];

        return list;
    }
}

public static class PaymentOptions
{
    public const string CASH = "CA";
    public const string CREDIT_CARD = "CC";
    public const string BANK_TRANSFER = "BT";
    public const string QR = "QR";
    public const string INTERNET_BANKING = "IB";
    public const string NA = "NA";

	public static string GetDisplayText(string? paymentOption)
	{
		return paymentOption switch
		{
			CASH => "Cash",
			CREDIT_CARD => "Credit Card",
			BANK_TRANSFER => "Bank Transfer",
			QR => "QR Payment",
            INTERNET_BANKING => "Internet Banking",
            NA => "Not Applicable",
            _ => "",
		};
	}

	public static List<DropdownSelectItem> GetForDropdownSelect()
	{
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem() { Key = CASH, Value = GetDisplayText(CASH) },
            new DropdownSelectItem() { Key = QR, Value = GetDisplayText(QR) },
            new DropdownSelectItem() { Key = BANK_TRANSFER, Value = GetDisplayText(BANK_TRANSFER) },
            new DropdownSelectItem() { Key = CREDIT_CARD, Value = GetDisplayText(CREDIT_CARD) },
            new DropdownSelectItem() { Key = INTERNET_BANKING, Value = GetDisplayText(INTERNET_BANKING) },
        ];
		return list;
	}
}

// FOR RETAIL MANAGEMENT SYSTEM
public static class ReceiptStatuses
{
    public const string NEW = "NEW";
    public const string DRAFT = "DRAFT";
    public const string PAID = "PAID";
    public const string OUTSTANDING = "OUTSTANDING";
    public const string VOID = "VOID";
    public const string CANCELLED = "CANCELLED";

    public static string GetDisplayText(string? status)
    {
        return status switch
        {
            NEW => "New",
            PAID => "Paid",
            OUTSTANDING => "Outstanding",
            VOID => "Void",
            CANCELLED => "Cancelled",
            _ => "",
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { NEW, GetDisplayText(NEW) },
            { PAID, GetDisplayText(PAID) },
            { OUTSTANDING, GetDisplayText(OUTSTANDING) },
            { VOID, GetDisplayText(VOID) },
            { CANCELLED, GetDisplayText(CANCELLED) },
        };
        return list;
    }

	public static List<DropdownSelectItem> GetForDropdownSelect()
	{
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem() { Key = NEW, Value = "New" },
			new DropdownSelectItem() { Key = PAID, Value = "Paid" },
			new DropdownSelectItem() { Key = OUTSTANDING, Value = "Outstanding" },
			new DropdownSelectItem() { Key = VOID, Value = "Void" },
			new DropdownSelectItem() { Key = CANCELLED, Value = "Cancelled" }
		];
		return list;
	}
}

public static class SupplierStatuses
{
    public const string ACTIVE = "ACTIVE";
    public const string INACTIVE = "INACTIVE";
    public const string CLOSED = "CLOSED";
    public const string TERMINATED = "TERMINATED";

    public static string GetDisplayText(string? status)
    {
        return status switch
        {
            ACTIVE => "Active",
            INACTIVE => "Inactive",
            CLOSED => "Closed",
            TERMINATED => "Terminated",
            _ => "",
        };
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem() { Key = ACTIVE, Value = "Active" },
            new DropdownSelectItem() { Key = INACTIVE, Value = "Inactive" },
            new DropdownSelectItem() { Key = CLOSED, Value = "Closed" },
            new DropdownSelectItem() { Key = TERMINATED, Value = "Terminated" }
        ];
        return list;
    }
}

public static class ProductUnitTypes
{
    public const string CONTAINER = "CONTAINER";
}

public static class RetailTaxTypes
{
    public const string VALUE_ADDED_TAX = "VAT";
    public const string WITHHOLDING_TAX = "WHT";

    public static string GetDisplayText(string? taxType)
    {
        return taxType switch
        {
            VALUE_ADDED_TAX => "VAT",
            WITHHOLDING_TAX => "Withholding Tax",
            _ => "",
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { VALUE_ADDED_TAX, GetDisplayText(VALUE_ADDED_TAX) },
            { WITHHOLDING_TAX, GetDisplayText(WITHHOLDING_TAX) },
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
        List<DropdownSelectItem> list = new()
        {
            new DropdownSelectItem() { Key = VALUE_ADDED_TAX, Value = GetDisplayText(VALUE_ADDED_TAX) },
            new DropdownSelectItem() { Key = WITHHOLDING_TAX, Value = GetDisplayText(WITHHOLDING_TAX) }
        };
        return list;
    }
}

#region WORKFLOW CONTROLLERS
/// <summary>
/// Workflow Controller - InventoryCheckIn
/// </summary>
public static class WFC_InventoryCheckIn
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
            { WorkflowStatuses.APPROVED + WorkflowActions.COMPLETE },
        };

        return list.Contains(currentWorkflowStatus + workflowAction);
    }

    public static string GetResultingWorkflowStatus(string workflowAction)
    {
        return workflowAction switch
        {
            WorkflowActions.SAVE_AS_DRAFT => WorkflowStatuses.DRAFT,
            WorkflowActions.SUBMIT_AND_COMPLETE => WorkflowStatuses.COMPLETE,
            WorkflowActions.SUBMIT_FOR_APPROVAL => WorkflowStatuses.PENDING_APPROVAL,
            WorkflowActions.SUBMIT_AND_APRPOVE => WorkflowStatuses.APPROVED,
            WorkflowActions.CANCEL => WorkflowStatuses.CANCELLED,
            WorkflowActions.QUERY => WorkflowStatuses.PENDING_REVISION,
            WorkflowActions.APPROVE => WorkflowStatuses.APPROVED,
            WorkflowActions.REJECT => WorkflowStatuses.REJECTED,
            _ => ""
        };
    }

    public static Dictionary<string, string> GetNextValidWorkflowActions(string currentWorkflowStatus, bool approvalRequired, bool isApprover=false)
    {
        Dictionary<string, string> list = [];

        switch (currentWorkflowStatus)
        {
            case WorkflowStatuses.START:
                {
                    list.Add(WorkflowActions.SAVE_AS_DRAFT, "Save As Draft");

                    if (approvalRequired)
                    {
                        if (isApprover)
                            list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "Submit & Approve");
                        else
                            list.Add(WorkflowActions.SUBMIT_FOR_APPROVAL, "Submit For Approval");
                    }
                    else
                    {
                        list.Add(WorkflowActions.SUBMIT_AND_COMPLETE, "Submit & Complete");
                    }
                }
                break;
            case WorkflowStatuses.DRAFT:
                {
                    if (approvalRequired)
                    {
                        if (isApprover)
                            list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "Submit & Approve");
                        else
                            list.Add(WorkflowActions.SUBMIT_FOR_APPROVAL, "Submit For Approval");
                    }
                    else
                    {
                        list.Add(WorkflowActions.SUBMIT_AND_COMPLETE, "Submit & Complete");
                    }

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
                    list.Add(WorkflowActions.COMPLETE, "Complete");
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
            new() { Key = WorkflowStatuses.DRAFT, Value="Draft" },
            new() { Key = WorkflowStatuses.PENDING_APPROVAL, Value = "Pending Approval" },
            new() { Key = WorkflowStatuses.PENDING_REVISION, Value = "Pending Revision" },
            new() { Key = WorkflowStatuses.APPROVED, Value = "Approved" },
            new() { Key = WorkflowStatuses.REJECTED, Value = "Rejected" },
            new() { Key = WorkflowStatuses.CANCELLED, Value = "Cancelled" },
            new() { Key = WorkflowStatuses.COMPLETE, Value = "Complete" }
        ];

        return list;
    }

    public static Dictionary<string, string> GetActionIconsList()
    {
        Dictionary<string, string> list = [];

        list.Add(WorkflowActions.SAVE_AS_DRAFT, "draft");
        list.Add(WorkflowActions.SUBMIT_AND_COMPLETE, "order_approve");
        list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "order_approve");
        list.Add(WorkflowActions.SUBMIT_FOR_APPROVAL, "schedule_send");
        list.Add(WorkflowActions.APPROVE, "cancel_schedule_send");
        list.Add(WorkflowActions.QUERY, "play_circle");
        list.Add(WorkflowActions.REJECT, "pause_circle");
        list.Add(WorkflowActions.CANCEL, "cancel");
        return list;
    }
}

public static class WFC_InventoryCheckOut
{
    public static bool IsValidWorkflowTransit(string currentWorkflowStatus, string workflowAction)
    {
        List<string> list = new()
        {
            { WorkflowStatuses.START + WorkflowActions.SAVE_AS_DRAFT },
            { WorkflowStatuses.START + WorkflowActions.SUBMIT_AND_COMPLETE },
            { WorkflowStatuses.START + WorkflowActions.SUBMIT_FOR_APPROVAL },
            { WorkflowStatuses.START + WorkflowActions.SUBMIT_AND_APRPOVE },
            { WorkflowStatuses.DRAFT + WorkflowActions.CANCEL },
            { WorkflowStatuses.DRAFT + WorkflowActions.SUBMIT_FOR_APPROVAL },
            { WorkflowStatuses.DRAFT + WorkflowActions.SUBMIT_AND_APRPOVE },
            { WorkflowStatuses.PENDING_REVISION + WorkflowActions.CANCEL },
            { WorkflowStatuses.PENDING_REVISION + WorkflowActions.SUBMIT_FOR_APPROVAL },
            { WorkflowStatuses.PENDING_APPROVAL + WorkflowActions.APPROVE },
            { WorkflowStatuses.PENDING_APPROVAL + WorkflowActions.QUERY },
            { WorkflowStatuses.PENDING_APPROVAL + WorkflowActions.REJECT },
            { WorkflowStatuses.APPROVED + WorkflowActions.COMPLETE },
        };

        return list.Contains(currentWorkflowStatus + workflowAction);
    }

    public static string GetResultingWorkflowStatus(string workflowAction)
    {
        return workflowAction switch
        {
            WorkflowActions.SAVE_AS_DRAFT => WorkflowStatuses.DRAFT,
            WorkflowActions.SUBMIT_AND_COMPLETE => WorkflowStatuses.COMPLETE,
            WorkflowActions.SUBMIT_FOR_APPROVAL => WorkflowStatuses.PENDING_APPROVAL,
            WorkflowActions.SUBMIT_AND_APRPOVE => WorkflowStatuses.APPROVED,
            WorkflowActions.CANCEL => WorkflowStatuses.CANCELLED,
            WorkflowActions.QUERY => WorkflowStatuses.PENDING_REVISION,
            WorkflowActions.APPROVE => WorkflowStatuses.APPROVED,
            WorkflowActions.REJECT => WorkflowStatuses.REJECTED,
            _ => ""
        };
    }

    public static Dictionary<string, string> GetNextValidWorkflowActions(string currentWorkflowStatus, bool approvalRequired, bool isApprover = false)
    {
        Dictionary<string, string> list = [];

        switch (currentWorkflowStatus)
        {
            case WorkflowStatuses.START:
                {
                    list.Add(WorkflowActions.SAVE_AS_DRAFT, "Save As Draft");

                    if (approvalRequired)
                    {
                        if (isApprover)
                            list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "Submit & Approve");
                        else
                            list.Add(WorkflowActions.SUBMIT_FOR_APPROVAL, "Submit For Approval");
                    }
                    else
                    {
                        list.Add(WorkflowActions.SUBMIT_AND_COMPLETE, "Submit & Complete");
                    }
                }
                break;
            case WorkflowStatuses.DRAFT:
                {
                    if (approvalRequired)
                    {
                        if (isApprover)
                            list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "Submit & Approve");
                        else
                            list.Add(WorkflowActions.SUBMIT_FOR_APPROVAL, "Submit For Approval");
                    }
                    else
                    {
                        list.Add(WorkflowActions.SUBMIT_AND_COMPLETE, "Submit & Complete");
                    }

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
            case WorkflowStatuses.COMPLETE:
                {
                    list.Add(WorkflowActions.SUBMIT_AND_APRPOVE, "Submit & Approve");
                }
                break;
            default:
                break;
        }

        return list;
    }

    public static List<DropdownSelectItem> GetWorkflowStatusForDropdownSelect()
    {
        List<DropdownSelectItem> list = [];
        list.Add(new DropdownSelectItem() { Key = WorkflowStatuses.DRAFT, Value = "Draft" });
        list.Add(new DropdownSelectItem() { Key = WorkflowStatuses.PENDING_APPROVAL, Value = "Pending Approval" });
        list.Add(new DropdownSelectItem() { Key = WorkflowStatuses.PENDING_REVISION, Value = "Pending Revision" });
        list.Add(new DropdownSelectItem() { Key = WorkflowStatuses.APPROVED, Value = "Approved" });
        list.Add(new DropdownSelectItem() { Key = WorkflowStatuses.REJECTED, Value = "Rejected" });
        list.Add(new DropdownSelectItem() { Key = WorkflowStatuses.CANCELLED, Value = "Cancelled" });

        return list;
    }
}

public static class WFC_CustomerPurchaseInvoice 
{
    public const string WFA_SafeDraft = "SAVE-DRAFT";
    public const string WFA_Cancel = "CANCEL";
    public const string WFA_Confirm = "CONFIRM";
    public const string WFA_SubmitForApproval = "SUBMIT-FOR-APPROVAL";
    public const string WFA_SubmitAndApprove = "SUBMIT-AND-APPROVE";
    public const string WFA_Approve = "APPROVE";
    public const string WFA_Reject = "REJECT";
    public const string WFA_Query = "QUERY";
    public const string WFA_Complete = "COMPLETE";

    public const string WFS_Start = "START";
    public const string WFS_Draft = "DRAFT";
    public const string WFS_PendingApproval = "PENDING-APPROVAL";
    public const string WFS_PendingRevision = "PENDING-REVISION";
    public const string WFS_Rejected = "REJECTED";
    public const string WFS_Cancelled = "CANCELLED";
    public const string WFS_Approved = "APPROVED";
    public const string WFS_Complete = "COMPLETE";

    public static bool IsValidWorkflowTransit(string currentWorkflowStatus, string workflowAction)
    {
        List<string> list = new()
        {
            { WFS_Start + WFA_SafeDraft },
            { WFS_Start + WFA_SubmitForApproval },
            { WFS_Draft + WFA_SubmitForApproval },
            { WFS_Draft + WFA_Cancel },
            { WFS_PendingApproval + WFA_Approve },
            { WFS_PendingApproval + WFA_Query },
            { WFS_PendingApproval + WFA_Reject },
            { WFS_PendingRevision + WFA_SubmitForApproval },
            { WFS_PendingRevision + WFA_Cancel },
        };

        return list.Contains(currentWorkflowStatus + workflowAction);
    }

    public static string GetResultingWorkflowStatus(string workflowAction)
    {
        return workflowAction switch
        {
            WFA_SafeDraft => WFS_Draft,
            WFA_SubmitForApproval => WFS_PendingApproval,
            WFA_SubmitAndApprove => WFS_Approved,
            WFA_Cancel => WFS_Cancelled,
            WFA_Query => WFS_PendingRevision,
            WFA_Approve => WFS_Approved,
            WFA_Reject => WFS_Rejected,
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

public static class WFC_CustomerPurchaseOrder 
{
    public const string WFA_SaveDraft = "SAVE-DRAFT";
    public const string WFA_Cancel = "CANCEL";
    public const string WFA_Confirm = "CONFIRM";
    public const string WFA_Approve = "APPROVE";
    public const string WFA_Reject = "REJECT";
    public const string WFA_Deliver = "DELIVER";
    public const string WFA_Receive = "RECEIVED";
    public const string WFA_Complete = "COMPLETE";

    public const string WFS_Start = "START";
    public const string WFS_Draft = "DRAFT";
    public const string WFS_Cancelled = "CANCELLED";
    public const string WFS_Confirmed = "CONFIRMED";
    public const string WFS_Delivering = "DELIVERING";
    public const string WFS_Delivered = "DELIVERED";
    public const string WFS_Complete = "COMPLETE";

    public static bool IsValidWorkflowTransit(string currentWorkflowStatus, string workflowAction)
    {
        List<string> list = new()
        {
            { WFS_Start + WFA_SaveDraft },
            { WFS_Start + WFA_Confirm },
            { WFS_Draft + WFA_Confirm },
            { WFS_Draft + WFA_Cancel },
            { WFS_Confirmed + WFA_Deliver },
            { WFS_Confirmed + WFA_Complete },
            { WFS_Delivering + WFA_Receive },
            { WFS_Delivered + WFA_Complete },
        };

        return list.Contains(currentWorkflowStatus + workflowAction);
    }

    public static string GetResultingWorkflowStatus(string workflowAction)
    {
        return workflowAction switch
        {
            WFA_SaveDraft => WFS_Draft,
            WFA_Cancel => WFS_Cancelled,
            WFA_Confirm => WFS_Confirmed,
            WFA_Complete => WFS_Complete,
            WFA_Deliver => WFS_Delivering,
            WFA_Receive => WFS_Delivered,
            _ => ""
        };
    }

    public static Dictionary<string, string> GetNextValidWorkflowActions(string currentWorkflowStatus, bool isDelivery = false)
    {
        Dictionary<string, string> list = [];

        switch (currentWorkflowStatus)
        {
            case WFS_Start:
                {
                    list.Add(WFA_SaveDraft, "Save As Draft");
                }
                break;
            case WFS_Draft:
                {
                    list.Add(WFA_Confirm, "Confirm Order");
                    list.Add(WFA_Cancel, "Cancel");
                }
                break;
            case WFS_Confirmed:
                {
                    if (isDelivery)
                    {
                        list.Add(WFA_Deliver, "Out for Delivery");
                    }
                    else
                    {
                        list.Add(WFA_Complete, "Order Complete");
                    }
                }
                break;
            case WFS_Delivering:
                {
                    list.Add(WFA_Receive, "Order Received");
                }
                break;
            case WFS_Delivered:
                {
                    list.Add(WFA_Complete, "Order Complete");
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
            new() { Key = WFS_Draft },
            new() { Key = WFS_Confirmed },
            new() { Key = WFS_Delivering },
        ];

        return list;
    }
}

public static class WFC_Order
{
    public const string WFA_SaveDraft = "SAVE-DRAFT";
    public const string WFA_Cancel = "CANCEL";
    public const string WFA_Confirm = "CONFIRM";
    public const string WFA_Deliver = "DELIVER";
    public const string WFA_Receive = "RECEIVED";
    public const string WFA_Complete = "COMPLETE";

    public const string WFS_Start = "START";
    public const string WFS_Draft = "DRAFT";
    public const string WFS_Cancelled = "CANCELLED";
    public const string WFS_Confirmed = "CONFIRMED";
    public const string WFS_Delivering = "DELIVERING";
    public const string WFS_Delivered = "DELIVERED";
    public const string WFS_Complete = "COMPLETE";

    public static string GetWorkflowStatusText(string workflowStatus)
    {
        return workflowStatus switch
        {
            WFS_Cancelled => "Cancelled",
            WFS_Complete => "Complete",
            WFS_Confirmed => "Confirmed",
            WFS_Delivered => "Delivered",
            WFS_Delivering => "Delivering",
            WFS_Draft => "Draft",
            WFS_Start => "Start",
            _ => "???"
        };
    }

    public static string GetActionext(string action)
    {
        return action switch
        {
            WFA_Cancel => "Cancel",
            WFA_Complete => "Complete",
            WFA_Confirm => "Confirm",
            WFA_Deliver => "Deliver",
            WFA_Receive => "Receive",
            WFA_SaveDraft => "Save As Draft",
            _ => "???"
        };
    }

    public static string GetResultingWorkflowStatus(string workflowAction)
    {
        return workflowAction switch
        {
            WFA_SaveDraft => WFS_Draft,
            WFA_Cancel => WFS_Cancelled,
            WFA_Confirm => WFS_Confirmed,
            WFA_Complete => WFS_Complete,
            WFA_Deliver => WFS_Delivering,
            WFA_Receive => WFS_Delivered,
            _ => ""
        };
    }

    public static Dictionary<string, string> GetNextValidWorkflowActions(string currentWorkflowStatus, bool isDelivery = false)
    {
        Dictionary<string, string> list = [];

        switch (currentWorkflowStatus)
        {
            case WFS_Start:
                {
                    list.Add(WFA_SaveDraft, "Save As Draft");
                }
                break;
            case WFS_Draft:
                {
                    list.Add(WFA_Confirm, "Confirm Order");
                    list.Add(WFA_Cancel, "Cancel");
                }
                break;
            case WFS_Confirmed:
                {
                    if (isDelivery)
                    {
                        list.Add(WFA_Deliver, "Out for Delivery");
                    }
                    else
                    {
                        list.Add(WFA_Complete, "Order Complete");
                    }
                }
                break;
            case WFS_Delivering:
                {
                    list.Add(WFA_Receive, "Order Received");
                }
                break;
            case WFS_Delivered:
                {
                    list.Add(WFA_Complete, "Order Complete");
                }
                break;
            default:
                break;
        }

        return list;
    }
}
#endregion
