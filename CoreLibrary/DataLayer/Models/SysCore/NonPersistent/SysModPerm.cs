/// <summary>
/// System Module Permission
/// </summary>
public class SysModPerm
{
	public string? ObjectCode { get; set; }
	public string? ObjectName { get; set; }
	public string? ObjectClassFullName { get; set; }
	public bool IsMenuGroup { get; set; }
	public string? ModulePath { get; set; }
	public bool CanCreate { get; set; }
	public bool CanRead { get; set; }
	public bool CanUpdate { get; set; }
	public bool CanDelete { get; set; }
	public bool CanProcess { get; set; }
	public bool IsAdmin { get; set; }

	public SysModPerm()
	{
		CanCreate = false;
		CanRead = false;
		CanUpdate = false;
		CanDelete = false;
		CanProcess = false;
		IsAdmin = false;
	}
}
