namespace DataLayer.Models.EventManagement.NonPersistent;

public class EventRegistrationSummary
{

    public string? EventCode { get; set; }
    public string? EventName { get; set; }
    public string? VenueName { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool IsSingleDayEvent { get; set; }

    public string? FeeTypeCode { get; set; }
    public string? EventType { get; set; }
    public int? TotalCollectedFeeAmountKhr { get; set; }
    public decimal? TotalCollectedFeeAmountUsd { get; set; }
    public int? TotalCollectedNoInviteFeeAmountKhr { get; set; }
    public decimal? TotalCollectedNoInviteFeeAmountUsd { get; set; }

    public int? TotalCollectedAbsenteeFeeAmountKhr { get; set; }
    public decimal? TotalCollectedAbsenteeFeeAmountUsd { get; set; }
    public int? RegisteredCount { get; set; }
    public int? RegisteredAbsenteeCount { get; set; }
    public int? RegisteredAttandenceCount { get; set; }
    public int? RegisteredRepresentativeCount { get; set; }
    public int? RegisteredNoInvitationCount { get; set; }
    public int? InvitationCount { get; set; }

    [Computed]
    [Description("ignore")]
    public string? TotalCollectedFeeAmountUsdText
    {
        get
        {
            return (this.TotalCollectedFeeAmountUsd ?? 0).ToString("#,##0.00");
        }
    }

    [Computed]
    [Description("ignore")]
    public string? TotalCollectedFeeAmountKhrText
    {
        get
        {
            return (this.TotalCollectedFeeAmountKhr ?? 0).ToString("#,##0");
        }
    }

    [Computed]
    [Description("ignore")]
    public string? TotalCollectedNoInviteFeeAmountKhrText
    {
        get
        {
            return (this.TotalCollectedNoInviteFeeAmountKhr ?? 0).ToString("#,##0");
        }
    }

    [Computed]
    [Description("ignore")]
    public string? TotalCollectedNoInviteFeeAmountUsdText
    {
        get
        {
            return (this.TotalCollectedNoInviteFeeAmountUsd ?? 0).ToString("#,##0.00");
        }
    }

    [Computed]
    [Description("ignore")]
    public string? TotalCollectedAbsenteeFeeAmountKhrText
    {
        get
        {
            return (this.TotalCollectedAbsenteeFeeAmountKhr ?? 0).ToString("#,##0.00");
        }
    }

    [Computed]
    [Description("ignore")]
    public string? TotalCollectedAbsenteeFeeAmountUsdText
    {
        get
        {
            return (this.TotalCollectedAbsenteeFeeAmountUsd ?? 0).ToString("#,##0.00");
        }
    }
}