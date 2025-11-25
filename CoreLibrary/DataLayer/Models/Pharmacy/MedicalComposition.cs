using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.Music;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Pharmacy;

[Table("[med].[MedicalComp]"), DisplayName("Medical Composition")]
public class MedicalComposition : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.PHARMACY;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => "MedicalComp";

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "medical_comp";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required(AllowEmptyStrings = false, ErrorMessage = "'CODE' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Item 'NAME' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

	public string? ObjectNameKh { get; set; }

	[StringLength(150), StringUnicode(true)]
    public string? FrenchName { get; set; }

    public string? TreatmentDescription { get; set; }

    public string? Remark { get; set; }

	public string? Note { get; set; }

    [DataType(DataType.Url)]
	public string? RefUrl { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    #endregion
}