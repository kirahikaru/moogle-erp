using DataLayer.GlobalConstant;

namespace DataLayer.Models.Retail.NonPersistent;

public class ItemCheckInOutHistory
{
    [Write(false), Computed]
    [Description("ignore"), ReadOnly(true)]
    public static string StoreProcedureName => $"[rms].[SP_GetInventoryCheckInCheckOutHistory]";

    #region *** DATABASE FIELDS ***
    public string? TransactionType { get; set; }
    public DateTime? TransactionDateTime { get; set; }
    public double? Quantity { get; set; }
    public string? UnitCode { get; set; }
    public string? UnitName { get; set; }
    public double? QtyAddition { get; set; }
    public double? QtySubtraction { get; set; }
    public DateTime? ApprovedDateTime { get; set; }
    public string? WorkflowStatus { get; set; }
    public int? Id { get; set; }

    [Write(false), Computed]
    public string WorkflowStatusText => WorkflowStatuses.GetDisplayText(WorkflowStatus);
    #endregion
}