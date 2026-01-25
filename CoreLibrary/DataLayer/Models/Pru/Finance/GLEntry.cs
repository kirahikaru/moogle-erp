using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Models.Pru.Finance;

[Table("[dbo].[GLEntry]"), DisplayName("GL Entry")]
public class GLEntry : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(GLEntry).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "gl_entry";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***

	[Required(AllowEmptyStrings = false, ErrorMessage = "'Account Code' is required.")]
    [RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'Account Code' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    [MaxLength(80)]
    public new string? ObjectCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "'Account Name' is required.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

	public DateTime? EffDate { get; set; }
	public DateTime? PostingDate { get; set; }
	public string? GlAccNo { get; set; }
	public string? FinActTrackerNo { get; set; }
	public string? LinkedObjType { get; set; }
	public int? LinkedObjId { get; set; }
	public DateTime? DocDate { get; set; }
	public string? DocType { get; set; }
	public string? DocRefNo { get; set; }
	public decimal? CR { get; set; }
	public decimal? DR { get; set; }

	#endregion

	#region *** LINKED OBJECTS ***

	#endregion

	#region *** DYNAMIC PROPERTIES ***
	#endregion

	public GLEntry() : base()
    {
		
    }
}