using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.RMS;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.PMS;

[Table("[med].[MedicalEquipment]"), DisplayName("Medical Equipment")]
public class MedicalEquipment : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.PHARMACY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "MedicalEquip";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "medical_equip";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'ID' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'ID' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Item 'NAME' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

	[StringLength(150), StringUnicode(true)]
	public string? ObjectNameKh { get; set; }
	
    public string? Brand { get; set; }
    public string? ModelNo { get; set; }
    public string? Barcode { get; set; }

    public string? Remark { get; set; }

	public string? Note { get; set; }
    public string? MfgCountryCode { get; set; }
    public int? ItemId { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
	public Item? Item { get; set; }

	[Computed, Write(false)]
	public Country? MfgCountry { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion
}