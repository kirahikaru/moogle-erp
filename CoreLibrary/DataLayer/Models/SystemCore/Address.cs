using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("Address")]
public class Address : AuditObject
{
    [Computed, ReadOnly(true), Write(false)]
    public new static string MsSqlTableName => $"[dbo].[{typeof(Address).Name}]";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"address";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? LinkedObjectId { get; set; }
    public string? LinkedRecordID { get; set; }
    public string? LinkedObjectType { get; set; }
    public string? Type { get; set; }
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? Line4 { get; set; }
    public string? Line5 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? District { get; set; }
    public string? SubDistrict { get; set; }
    public string? Zipcode { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public string? Landmark { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}