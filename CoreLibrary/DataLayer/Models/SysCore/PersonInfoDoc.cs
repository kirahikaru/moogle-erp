using Dapper.Contrib.Extensions;
using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataAnnot = System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Models.SysCore;

[Table("PersonInfoDoc")]
public class PersonInfoDoc : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"{typeof(PersonInfoDoc).Name}";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"person_info_doc";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? PersonId { get; set; }
	public string? PersonCode { get; set; }
	public int? DocumentTypeId { get; set; }
	public string? DocumentTypeCode { get; set; }

	public int? DocumentId { get; set; }
	public string? DocumentCode { get; set; }
	public DateTime? IssuedDate { get; set; }
	public DateTime? ExpiryDate { get; set; }
	public string? IssuingCountryCode { get; set; }
	public string? Remark { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Country? IssuingCountry { get; set; }

    [Computed, Write(false)]
    public DocumentType? DocumentType { get; set; }

	[Computed, Write(false)]
	public Document? Document { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    
    #endregion

    public PersonInfoDoc() : base()
    {

    }
}