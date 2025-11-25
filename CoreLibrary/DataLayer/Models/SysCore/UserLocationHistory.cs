using DataLayer.AuxComponents.DataAnnotations;

namespace DataLayer.Models.SysCore;

[Table("UserLocHistory"), DisplayName("User Location History")]
public class UserLocationHistory : AuditObject
{
	[Computed, Write(false), ReadOnly(false)]
	public new static string MsSqlTableName => $"UserLocHistory";

	[Computed, Write(false), ReadOnly(false)]
	public new static string PgTableName => "user_loc_history";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	//public new Int64 Id { get; set; }
	public string? AppCode { get; set; }
    public int? UserId { get; set; }
    public string? UserUserID { get; set; }
    public string? Username { get; set; }
    public string? EventName { get; set; }
    [MaxLength(150), StringUnicode(true)]
    public string? DistrictName { get; set; }

    [MaxLength(150), StringUnicode(true)]
    public string? SubDistrictName { get; set; }
    
    public string? Zipcode { get; set; }
    [MaxLength(150), StringUnicode(true)]
    public string? CountryName { get; set; }

    public string? CountryCode { get; set; }

    [MaxLength(150), StringUnicode(true)]
    public string? Landmark { get; set; }
    public int? DistanceAccuracy { get; set; }
    [Precision(9, 6)]
    public double? Latitude { get; set; }
    [Precision(9, 6)]
    public double? Longitude { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}