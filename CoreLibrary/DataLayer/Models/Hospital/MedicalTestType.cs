using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Hospital;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[hms].[MedicalTestType]"), DisplayName("Medical Test Type")]
public class MedicalTestType : AuditObject, IParentChildHierarchyObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(MedicalTestType).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "medical_test_type";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Code' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Code' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [RegularExpression(@"^[a-zA-Z\d\s\W]{0,}$", ErrorMessage = "'Name' invalid format.")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "'Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
	public string? ObjectNameKh { get; set; }
    public string? Desc { get; set; }
    public int? ParentId { get; set; }
    public string? ParentCode { get; set; }
    public string? HierarchyPath { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
	public MedicalTestType? Parent { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}