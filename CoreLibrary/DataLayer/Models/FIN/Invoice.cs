using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.FIN;

[Table("[fin].[Invoice]")]
public class Invoice : WorkflowEnabledObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.FINANCE;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Invoice).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "invoice";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELD ***
	public int? CustomerId { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public decimal? DepositAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? TotalDiscountAmount { get; set; }
    public decimal? TotalPayableAmount { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Customer? Customer { get; set; }

	[Computed, Write(false)]
	public List<InvoiceItem> Items { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion

    public Invoice()
    {
        Items = [];
    }
}