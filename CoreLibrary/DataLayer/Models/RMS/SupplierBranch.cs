using DataLayer.GlobalConstant;

namespace DataLayer.Models.RMS;

[Table("[rms].[SupplierBranch]")]
public class SupplierBranch : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.RETAIL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(SupplierBranch).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "supplier_branch";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? SupplierId { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? PhoneLine1 { get; set; }

    [MaxLength(50)]
    public string? PhoneLine2 { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
	public List<Contact> Contacts { get; set; }

	[Computed, Write(false)]
	public Address? Address { get; set; }

	[Computed, Write(false)]
	public CambodiaAddress? CambodiaAddress { get; set; }
    #endregion

    public SupplierBranch() : base()
    {
        Contacts = [];
    }
}