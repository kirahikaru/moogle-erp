using DataLayer.GlobalConstant;
using DataLayer.Models.Finance;
using DataLayer.Models.SystemCore;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.Hospital;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
[Table("[hms].[MedicalExamItem]")]
public class MedicalExamItem : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.HOSPITAL;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(MedicalExamItem).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "medical_exam_item";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? MedicalExamId { get; set; }
    public int? MedicalTestId { get; set; }
    public string? MedicalTestCode { get; set; }
    public int? MedicalTestTypeId { get; set; }
    public string? MedicalTestTypeCode { get; set; }
    public string? ResultDesc { get; set; }
    public decimal? ResultValue { get; set; }
    public string? Remark { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    [Computed, Write(false)]
    public Customer? Customer { get; set; }

	[Computed, Write(false)]
	public Doctor? Doctor { get; set; }

	[Computed, Write(false)]
	public User? RequestorUser { get; set; }

	[Computed, Write(false)]
	public UnitOfMeasure? ValueUom { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***

    #endregion
}