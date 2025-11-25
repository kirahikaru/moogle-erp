using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SysCore;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.PMS;

[Table("[med].[MedicineComp]"), DisplayName("Medicine Composition")]
public class MedicineComposition : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.PHARMACY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "MedicineComp";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "medicine_comp";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? MedicineId { get; set; }
	public string? MedicineCode { get; set; }
	public int? OrderNo { get; set; }
    [Required(ErrorMessage = "'Composition' is required.")]
    public int? MedicalCompositionId { get; set; }
	public string? MedicalCompositionCode { get; set; }
	public string? FrenchName { get; set; }

    [Required(ErrorMessage = "'Unit' is required.")]
    [MaxLength(25)]
    public string? UnitCode { get; set; }

    [Required(ErrorMessage = "'Quantity' is required.")]
    [Range(0.00, 99999999999.99, ErrorMessage ="'Quantity' must be positive number.")]
    [Precision(10, 2)]
    public double? Quantity { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public MedicalComposition? MedicalComposition { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? Unit { get; set; }
	#endregion

	#region *** DYNAMIC FIELDS ***
	[Computed, Write(false), ReadOnly(true)]
	public string QuantityText {
        get
        {
            if (Quantity.HasValue)
            {
                if (Quantity % 1 > 0)
                    return Quantity.Value.ToString("#,##0.00");
                else
                    return Quantity.Value.ToString("#,##0");
            }
            else
                return "-";
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string QuantityWithUnitText => QuantityText + (Unit != null ? $" {Unit.UnitSymbol}" : "");
    
    #endregion
}