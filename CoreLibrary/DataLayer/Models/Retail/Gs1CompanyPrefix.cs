using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Retail;


/// <summary>
/// GS1 Company Prefix
/// https://www.gs1.org/standards/id-keys/company-prefix
/// </summary>
[Table("[rms].[Gs1CompanyPfx]")]
public class Gs1CompanyPrefix : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "Gs1CompanyPfx";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "gs1_company_pfx";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);


	#region *** DATABASE FIELDS ***
	public int? StartNumber { get; set; }
    public int? EndNumber { get; set; }
    public string? CountryCode { get; set; }
    public string? CountryName { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
	public Country? Country { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}