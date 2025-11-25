using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("[dbo].[BusnSector]"), DisplayName("Business Sector")]
public class BusinessSector : AuditObject, IParentChildHierarchyObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"BusnSector";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"busn_sector";

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

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Name' is required.")]
    [RegularExpression(@"^[a-zA-Z\d\s\W]{0,}$", ErrorMessage = "'Name' is invalid format.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }
    public string? ObjectNameKh { get; set; }
    public int? ParentId { get; set; }
    public string? ParentCode { get; set; }
    public string? HierarchyPath { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public BusinessSector? Parent { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion
}