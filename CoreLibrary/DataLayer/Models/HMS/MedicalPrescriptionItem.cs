using DataLayer.GlobalConstant;

namespace DataLayer.Models.HMS;

[Table("[hms].[MedicalRxItem]")]
public class MedicalPrescriptionItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "MedicalRxItem";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "medical_rx_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	//public int? MedicalPrescriptionId { get; set; }
	public int? MedicalRxId { get; set; }
    public int? MedicineId { get; set; }
    public string? UnitCode { get; set; }
    public int? Quantity { get; set; }
    public string? DosageDirectionDesc { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Medicine? Medicine { get; set; }

    [Computed, Write(false)]
    public UnitOfMeasure? Unit { get; set; }

    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}