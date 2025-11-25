using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;
/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("DropdownDataList"), DisplayName("Dropdown Data List Setup")]
public class DropdownDataList : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(DropdownDataList).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"dropdown_datalist";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'CODE' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'ObjectName' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Object Field Name' is required.")]
    public string? ObjectFieldName { get; set; }
    
    /// <summary>
    /// GlobalCostants SystemCore > DropdownDataSystems
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "'System Name' is required.")]
    public string? SystemName { get; set; }
    
    [Required(ErrorMessage ="'Name' is required.")]
	[RegularExpression(@"^[a-zA-Z\s\d._-]{0,}$", ErrorMessage = "'Name' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	public string? NameEn { get; set; }
    public string? NameKh { get; set; }

    [DefaultValue(true)]
    public bool IsEnabled { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion

    public DropdownDataList()
    {
        IsEnabled = true;
    }
}