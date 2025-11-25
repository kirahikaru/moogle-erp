using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.PruCORE;

[Table("[dbo].[PruCoreProject]"), DisplayName("Project")]
public class PruCoreProject : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(PruCoreProject).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "gl_account";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public string? AzureLbuCodes { get; set; }
	public string? Description { get; set; }
	public string? BusnFunc { get; set; }

	public string? Owner { get; set; }
	public string? ManagersGroup { get; set; }
	public string? ContributorsGroup { get; set; }
	public string? ViewersGroup { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed]
	public List<PruCoreInfraStack> InfraStacks { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion

	public PruCoreProject()
	{
		InfraStacks = [];
	}
}