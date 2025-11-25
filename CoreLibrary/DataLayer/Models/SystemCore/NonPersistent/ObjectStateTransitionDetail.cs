namespace DataLayer.Models.SystemCore.NonPersistent;

public class ObjectStateTransitionDetail
{
    public int? ObjectId { get; set; }
    public string? ObjectType { get; set; }
    public int? TargetUserId { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? ActionCode { get; set; }
	public string? ActionText { get; set; }
	public string? CurrentState{ get; set; }
    public string? CurrentStateText { get; set; }
    public string? TargetState { get; set; }
	public string? TargetStateText { get; set; }
	public string? TransitionRemark { get; set; }
    public User? TargetUser { get; set; }
}