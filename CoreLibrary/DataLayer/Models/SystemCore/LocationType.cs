using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[Table("LocationType"), DisplayName("Location Type")]
public class LocationType : AuditObject, IParentChildHierarchyObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(LocationType).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"location_type";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[MaxLength(255), StringUnicode(true)]
    public string? LocalName { get; set; }

    public int? ParentId { get; set; }
    public string? ParentCode { get; set; }
    public string? HierarchyPath { get; set; }
    public bool IsEnabled { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public LocationType? Parent { get; set; }

	[Computed, Write(false)]
	public List<LocationType> Childs { get; set; }

	[Computed, Write(false)]
	public List<Location> Locations { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion

    public LocationType() : base()
    {
        IsEnabled = true;
		Childs = [];
		Locations = [];
    }
}