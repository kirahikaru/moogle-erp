using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.IT;

[Table("[dbo].[ITAssetServerInfo]"), DisplayName("IT Asset - Server Detail")]
public class ITAssetServerInfo : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(ITAssetServerInfo).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "it_asset_server_info";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public new string? ObjectCode { get; set; }
    public new string? ObjectName { get; set; }
	public int? ITAssetId { get; set; }
	public string? Env { get; set; }

	/// <summary>
	/// Azure / On-Prem
	/// </summary>
	public string? ZoneType { get; set; }
	public string? AppName { get; set; }
	public string? OwnerName { get; set; }
	/// <summary>
	/// Operating System
	/// </summary>
	public string? OS { get; set; }

	public string? IPAddr { get; set; }
	public string? UsagePurpose { get; set; }
	public string? Remark { get; set; }
	public bool IsActive { get; set; }
	public bool SCCMClientInd { get; set; }
	public string? Domain { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYNAMIC PROPERTIES ***
	
	#endregion

	public ITAssetServerInfo() : base()
    {
		
    }
}