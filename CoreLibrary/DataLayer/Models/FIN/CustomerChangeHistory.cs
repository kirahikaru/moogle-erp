using DataLayer.GlobalConstant;

namespace DataLayer.Models.FIN;

[Table("[fin].[CustChgHistory]"), DisplayName("Customer Change History")]
public class CustomerChangeHistory : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.FINANCE;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "CustChgHistory";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "cust_chg_history";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? CustomerId { get; set; }

    /// <summary>
    /// GlobalConstants > FIN > CustomerUpdateActions
    /// </summary>
    public string? ActionType { get; set; }
    public string? Remark { get; set; }
    public int? TriggeredUserId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    
    #endregion
}