namespace DataLayer.Models.SysCore;

[Table("OrgStruct"), DisplayName("Orgainzation Structure")]
public class OrgStruct : AuditObject, IParentChildHierarchyObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(OrgStruct).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"org_struct";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ObjectNameKh { get; set; }

	[Required(ErrorMessage = "'Organization Structure Type' is required.")]
	public int? OrgStructTypeId { get; set; }
	public string? OrgStructTypeCode { get; set; }
    public bool IsEnabled { get; set; }
	public int? ParentId { get; set; }
	public string? ParentCode { get; set; }
	public string? HierarchyPath { get; set; }

	/// <summary>
	/// Valid Values > GlobalConstants.ConfidentialityLevels
	/// </summary>
	//public int DefaultConfidentialityLevel { get; set; }
	public int DefaultPrivacyLevel { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public OrgStructType? Type { get; set; }

	[Computed, Write(false)]
	public OrgStruct? Parent { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, ReadOnly(true)]
	public string ParentName => Parent != null ? Parent.ObjectName.NonNullValue("-") : "-";
	#endregion

	public OrgStruct()
	{
		IsEnabled = true;
	}
}