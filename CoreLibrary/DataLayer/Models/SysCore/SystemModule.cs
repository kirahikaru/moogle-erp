namespace DataLayer.Models.SysCore;

[Table("SystemModule"), DisplayName("System Module")]
public class SystemModule : AuditObject, IParentChildHierarchyObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(SystemModule).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"system_module";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(ErrorMessage = "'Code' is required.")]
	[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	[MaxLength(80)]
	public new string? ObjectCode { get; set; }

	/// <summary>
	/// Module Name
	/// </summary>
	[RegularExpression(@"^[a-zA-Z\s\d\W]{0,}$", ErrorMessage = "'Name' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	[Required(ErrorMessage = "'Name' is required.")]
	[MaxLength(255)]
	public new string? ObjectName { get; set; }

	[Required(ErrorMessage = "'Module Path' is required.")]
	public string? ModulePath { get; set; }
	public bool IsMenuGroup { get; set; }
	public bool IsEnabled { get; set; }
	public string? ObjectClassFullName { get; set; }
	public string? ObjectClassName { get; set; }
	public bool? IsWorkflowObject { get; set; }

	public int? ParentId { get; set; }
	public string? ParentCode { get; set; }
	public string? HierarchyPath { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public SystemModule? Parent { get; set; }

	[Computed, Write(false)]
	public List<RoleSysMod> AssignedRoles { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion

	public SystemModule()
	{
		AssignedRoles = [];
		IsEnabled = true;
		IsMenuGroup = false;
	}
}