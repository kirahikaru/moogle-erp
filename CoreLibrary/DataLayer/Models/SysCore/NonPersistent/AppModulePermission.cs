namespace DataLayer.Models.SysCore.NonPersistent;

public class AppModulePermission
{
    public int? UserId { get; set; }
    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public string ModuleName { get; set; }
    public string ModulePath { get; set; }

    public AppModulePermission()
    {
        CanCreate = false;
        CanRead = false;
        CanUpdate = false;
        CanDelete = false;
        ModuleName = string.Empty;
        ModulePath = string.Empty;
    }
}
