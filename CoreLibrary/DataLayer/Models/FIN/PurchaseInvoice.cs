using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.FIN;

[Table("[rms].[PurchaseInvoice]")]
public class PurchaseInvoice : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(PurchaseInvoice).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "purchase_invoice";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public DateTime? BusinessDate { get; set; }

    [MaxLength(20)]
    public string? SupplierCode { get; set; }

    [MaxLength(3)]
    public string? CurrencyCode { get; set; }

    [Precision(18, 2)]
    public decimal? TotalPayableAmount { get; set; }

    [MaxLength(25)]
    public string? Status { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC FIELDS ***
    #endregion
}