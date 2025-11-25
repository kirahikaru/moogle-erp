using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Procurement;

[Table("[rms].[PurchaseInvoiceItem]")]
public class PurchaseInvoiceItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(PurchaseInvoiceItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "purchase_invoice_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[MaxLength(25)]
    public string? InvoiceNumber { get; set; }
    public int? InvoiceId { get; set; }
    public int? ItemId { get; set; }

    public string? ItemCode { get; set; }

    [Precision(18, 2)]
    public double Quantity { get; set; }

    [Precision(18, 2)]
    public decimal? UnitPrice { get; set; }

    [Precision(18, 2)]
    public decimal? TotalAmount { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    #endregion

    #region *** DYNAMIC FIELDS ***
    #endregion
}