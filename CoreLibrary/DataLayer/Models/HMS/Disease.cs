using DataLayer.GlobalConstant;

namespace DataLayer.Models.HMS;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[hms].[Disease]")]
public class Disease : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Disease).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "disease";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'Name' is required.")]
	public new string? ObjectName { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "'ID' is required.")]
	public new string? ObjectCode { get; set; }
	public string? ObjectNameKh { get; set; }
    
    #endregion

    #region *** LINKED OBJECTS ***
    
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}