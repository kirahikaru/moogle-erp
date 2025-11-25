using DataLayer.Models;
using Microsoft.Extensions.Primitives;

namespace DataLayer.GlobalConstant;

#region EMS - Event Management System
public static class EventFeeTypes
{
    public const string FREE = "FREE";
    public const string FIXED_FEE = "FIXED_FEE";
    public const string DONATION = "DONATION";

    public static string GetDisplayText(string? eventTypeCode)
    {
        return eventTypeCode switch
        {
            FREE => "Free Admimission",
            FIXED_FEE => "Fixed Fee",
            DONATION => "Donation",
            _ => "-"
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { FREE, GetDisplayText(FREE) },
            { FIXED_FEE, GetDisplayText(FIXED_FEE) },
            { DONATION, GetDisplayText(DONATION) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = FREE, Value = GetDisplayText(FREE) },
            new DropdownSelectItem { Key = FIXED_FEE, Value = GetDisplayText(FIXED_FEE) },
            new DropdownSelectItem { Key = DONATION, Value = GetDisplayText(DONATION) }
        ];

        return list;
    }
}

/// <summary>
/// 
/// </summary>
public static class EventInvitationStatuses
{
    public const string PENDING = "PENDING";

    /// <summary>
    /// 
    /// </summary>
    public const string REJECTED = "REJECTED";

    /// <summary>
    /// Invation has been sent
    /// </summary>
    public const string INVITED = "INVITED";

    /// <summary>
    /// Invitee has come and registered at the event
    /// </summary>
    public const string REGISTERED = "REGISTERED";

    /// <summary>
    /// Invitation cancelled
    /// </summary>
    public const string CANCELLED = "CANCELLED";
    

    public static string GetDisplayText(string? eventTypeCode)
    {
        return eventTypeCode switch
        {
            PENDING => "Pending",
            REJECTED => "Rejected",
            INVITED => "Invited",
            REGISTERED => "Registered",
            CANCELLED => "Cancelled",
            _ => ""
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { PENDING, GetDisplayText(PENDING) },
            { REJECTED, GetDisplayText(REJECTED) },
            { INVITED, GetDisplayText(INVITED) },
            { REGISTERED, GetDisplayText(REGISTERED) },
            { CANCELLED, GetDisplayText(CANCELLED) }
        };

        return list;
    }

    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem { Key = PENDING, Value = GetDisplayText(PENDING) },
            new DropdownSelectItem { Key = REJECTED, Value = GetDisplayText(REJECTED) },
            new DropdownSelectItem { Key = INVITED, Value = GetDisplayText(INVITED) },
            //new DropdownSelectItem { Key = REGISTERED, Value = GetDisplayText(REGISTERED) },
            new DropdownSelectItem { Key = CANCELLED, Value = GetDisplayText(CANCELLED) }
        ];

        return list;
    }
}


/// <summary>
/// Event Registration Attendance Codes
/// </summary>
public static class EventRegAttndCodes
{
    public const string PRESENT = "P";
    public const string ABSENT = "A";
    public const string REPRESENTATIVE = "R";

    public static string GetDisplayText(string? attendanceCode)
    {
        return attendanceCode switch
        {
            PRESENT => "Present",
            ABSENT => "Absent",
            REPRESENTATIVE => "Representative",
            _ => ""
        };
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
        List<DropdownSelectItem> list = new()
        {
            new() { Id = 0, Key = PRESENT, Value = "Present" },
            new() { Id = 0, Key = ABSENT, Value = "Absent" },
            new() { Id = 0, Key = REPRESENTATIVE, Value = "Reppresentative" }
        };

        return list;
    }
}

public static class EventWorkflowStatuses
{
    public const string START = "START";
    public const string DRAFT = "DRAFT";
    public const string CANCELLED = "CANCELLED";
    public const string REGISTERED = "REGISTERED";
    public const string INVITATION_OPEN = "INVITATION-OPEN";
    public const string INVITATION_CLOSE = "INVITATION-CLOSE";
    public const string REGISTRATION_OPEN = "REGISTRATION-OPEN";
    public const string REGISTRATION_HOLD = "REGISTRATION-HOLD";
    public const string REGISTRATION_CLOSED = "REGISTRATION-CLOSED";
    public const string COMPLETE = "COMPLETE";

    public static string GetDisplayText(string? workflowStatus)
    {
        return workflowStatus switch
        {
            START => "Start",
            DRAFT => "Draft",
            CANCELLED => "Cancelled",
            REGISTERED => "Registered",
            INVITATION_OPEN => "Invitation Open",
            INVITATION_CLOSE => "Invitation Closed",
            REGISTRATION_OPEN => "Registration Open",
            REGISTRATION_HOLD => "Registration Held",
            REGISTRATION_CLOSED => "Registration Closed",
            COMPLETE => "Complete",
            _ => ""
        };
    }
}

public static class EventWorkflowActions
{
    public const string REGISTER = "REGISTER";
    public const string SAVE_AS_DRAFT = "SAVE-DRAFT";
    public const string START_INVITATION = "START-INVITATION";
    public const string CLOSE_INVITATION = "CLOSE-INVITATION";
    public const string START_REGISTRATION = "START-REGISTRATION";
    public const string HOLD_REGISTRATION = "HOLD-REGISTRATION";
    public const string RESUME_REGISTRATION = "RESUME-REGISTRATION";
    public const string END_REGISTRATION = "END-REGISTRATION";
    public const string CLOSE = "CLOSE";
    public const string CANCEL = "CANCEL";
    
    public static string GetDisplayText(string workflowAction)
    {
        return workflowAction switch
        {
            REGISTER => "Register",
            SAVE_AS_DRAFT => "Save As Draft",
            START_INVITATION => "Start Invitation",
            CLOSE_INVITATION => "Close Invitation",
            START_REGISTRATION => "Start Registration",
            HOLD_REGISTRATION => "Hold Registration",
            END_REGISTRATION => "End Registration",
            CLOSE => "Close",
            CANCEL => "Cancel",
            _ => "????"
        };
    }

    public static Dictionary<string, string> GetActionIconsList()
    {
        Dictionary<string, string> list = [];

        list.Add(REGISTER, "person");
        list.Add(SAVE_AS_DRAFT, "draft");
        list.Add(START_INVITATION, "schedule_send");
        list.Add(CLOSE_INVITATION, "cancel_schedule_send");
        list.Add(START_REGISTRATION, "play_circle");
        list.Add(END_REGISTRATION, "pause_circle");
        list.Add(CLOSE, "recommend");
        list.Add(CANCEL, "cancel");
        return list;
    }
}

public static class EventWorkflowController
{
    public static bool IsValidWorkflowTransit(string currentWorkflowStatus, string workflowAction)
    {
        List<string> list = new()
        {
            { EventWorkflowStatuses.START + EventWorkflowActions.REGISTER },
            { EventWorkflowStatuses.START + EventWorkflowActions.SAVE_AS_DRAFT },
            { EventWorkflowStatuses.DRAFT + EventWorkflowActions.REGISTER },
            { EventWorkflowStatuses.REGISTERED + EventWorkflowActions.START_INVITATION },
            { EventWorkflowStatuses.INVITATION_OPEN + EventWorkflowActions.CLOSE_INVITATION },
            { EventWorkflowStatuses.INVITATION_CLOSE + EventWorkflowActions.START_REGISTRATION },
            { EventWorkflowStatuses.REGISTRATION_OPEN + EventWorkflowActions.HOLD_REGISTRATION },
            { EventWorkflowStatuses.REGISTRATION_OPEN + EventWorkflowActions.END_REGISTRATION },
            { EventWorkflowStatuses.REGISTRATION_HOLD + EventWorkflowActions.RESUME_REGISTRATION },
            { EventWorkflowStatuses.REGISTRATION_HOLD + EventWorkflowActions.END_REGISTRATION },
            { EventWorkflowStatuses.REGISTRATION_CLOSED + EventWorkflowActions.CLOSE },
        };

        return list.Contains(currentWorkflowStatus + workflowAction);
    }

    public static string GetResultingWorkflowStatus(string workflowAction)
    {
        return workflowAction switch
        {
            EventWorkflowActions.SAVE_AS_DRAFT => EventWorkflowStatuses.DRAFT,
            EventWorkflowActions.CANCEL => WorkflowStatuses.CANCELLED,
            EventWorkflowActions.REGISTER => EventWorkflowStatuses.REGISTERED,
            EventWorkflowActions.START_INVITATION => EventWorkflowStatuses.INVITATION_OPEN,
            EventWorkflowActions.CLOSE_INVITATION => EventWorkflowStatuses.INVITATION_CLOSE,
            EventWorkflowActions.START_REGISTRATION => EventWorkflowStatuses.REGISTRATION_OPEN,
            EventWorkflowActions.HOLD_REGISTRATION => EventWorkflowStatuses.REGISTRATION_HOLD,
            EventWorkflowActions.RESUME_REGISTRATION => EventWorkflowStatuses.REGISTRATION_OPEN,
            EventWorkflowActions.END_REGISTRATION => EventWorkflowStatuses.REGISTRATION_CLOSED,
            EventWorkflowActions.CLOSE => EventWorkflowStatuses.COMPLETE,
            _ => ""
        };
    }

    public static Dictionary<string, string> GetNextValidWorkflowActions(string currentWorkflowStatus)
    {
        Dictionary<string, string> list = [];

        switch (currentWorkflowStatus)
        {
            case EventWorkflowStatuses.START:
                {
                    list.Add(EventWorkflowActions.SAVE_AS_DRAFT, "Safe As Draft");
                    list.Add(EventWorkflowActions.REGISTER, "Register");
                }
                break;
            case EventWorkflowStatuses.DRAFT:
                {
                    list.Add(EventWorkflowActions.REGISTER, "Register");
                    list.Add(EventWorkflowActions.CANCEL, "Cancel");
                }
                break;
            case EventWorkflowStatuses.REGISTERED:
                {
                    list.Add(EventWorkflowActions.START_INVITATION, "Start Invitation");
                }
                break;
            case EventWorkflowStatuses.INVITATION_OPEN:
                {
                    list.Add(EventWorkflowActions.CLOSE_INVITATION, "Close Invitation");
                } break;
            case EventWorkflowStatuses.INVITATION_CLOSE:
                {
                    list.Add(EventWorkflowActions.START_REGISTRATION, "Start Registration");
                } break;
            case EventWorkflowStatuses.REGISTRATION_OPEN:
                {
                    list.Add(EventWorkflowActions.HOLD_REGISTRATION, "Hold Registration");
                    list.Add(EventWorkflowActions.END_REGISTRATION, "Close Registration");
                }
                break;
            case EventWorkflowStatuses.REGISTRATION_HOLD:
                {
                    list.Add(EventWorkflowActions.RESUME_REGISTRATION, "Resume Registration");
                    list.Add(EventWorkflowActions.END_REGISTRATION, "Close Registration");
                }
                break;
            case EventWorkflowStatuses.REGISTRATION_CLOSED:
                {
                    list.Add(EventWorkflowActions.CLOSE, "Close Event");
                }
                break;
            default:
                break;
        }

        return list;
    }
}
#endregion
