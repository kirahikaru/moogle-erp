using DataLayer.GlobalConstant;

namespace DataLayer.Models.SysCore;


//[Table("[dbo].[CambodiaCountryStructure]")]
[Table("KhCountryStruct"), DisplayName("Cambodia Country Structure")]
public class CambodiaCountryStructure : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"KhCountryStruct";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"kh_country_struct";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? ObjectNameKh { get; set; }
    [MaxLength(2)]
    public string? Code2 { get; set; }
    [MaxLength(3)]
    public string? Code3 { get; set; }
    public int Level { get; set; }
    public int? ParentId { get; set; }
    public string? HierarchyPath { get; set; }
    public string? PostalCode { get; set; }
    public bool IsEnabled { get; set; }
    /// <summary>
    /// Valid values in GlobalConstants.SystemCore > CambodiaCountryStructureTypes
    /// </summary>
    public string? TypeCode { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false), ReadOnly(true)]
	public CambodiaCountryStructure? Parent { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, Write(false), ReadOnly(true)]
	public string TypeText => CambodiaCtyStructTypes.GetDisplayText(TypeCode);
	#endregion

	public CambodiaCountryStructure()
    {
        IsEnabled = true;
    }
}