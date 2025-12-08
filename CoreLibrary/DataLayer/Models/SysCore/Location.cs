using DataLayer.AuxComponents.DataAnnotations;

namespace DataLayer.Models.SysCore;

[Table("Location")]
public class Location : AuditObject, IParentChildHierarchyObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(Location).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"location";

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

    [Required(AllowEmptyStrings = false, ErrorMessage = "Location name is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

    [MaxLength(255), StringUnicode(true)]
    public string? LocalName { get; set; }

    [Required(ErrorMessage = "'Location Type' is required.")]
    public int? LocationTypeId { get; set; }
	public string? LocationTypeCode { get; set; }
	//public string? LocationTypeCode { get; set; }
	public string? ReferenceNumber { get; set; }
    public int? ParentId { get; set; }
	public string? ParentCode { get; set; }
	public string? HierarchyPath { get; set; }

    [Required(ErrorMessage = "'Start Date' is required")]
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Location? Parent { get; set; }

	[Computed, Write(false)]
	public List<Location> Childs { get; set; }

	[Computed, Write(false)]
	public CambodiaAddress? CambodiaAddress { get; set; }

	[Computed, Write(false)]
	public Address? Address { get; set; }

	[Computed, Write(false)]
	public LocationType? LocationType { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	[Computed, ReadOnly(true), Write(false)]
	public string ObjectNameAndCode => $"{ObjectName.NonNullValue("-")} ({ObjectCode.NonNullValue("-")})";
    #endregion

    public Location() : base()
    {
        Childs = [];
    }
}