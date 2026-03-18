using Dapper.Contrib.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Models;

public class AuditObject
{
    [NotMapped]
    public static string SchemaName => "";

    [NotMapped]
    /// <summary>
    /// Microsoft SQL Service Full Table Name
    /// </summary>
    public static string MsSqlTableName => $"{typeof(AuditObject).Name}";

    /// <summary>
    /// PostgreSQL Full Table Name
    /// </summary>
    public static string PgTableName => $"{typeof(AuditObject).Name.ToLower()}";

    [Dapper.Contrib.Extensions.Key]
    [System.ComponentModel.DataAnnotations.Key]
	[Column("id")]
	public int Id { get; set; }

	//[Required(AllowEmptyStrings = false, ErrorMessage = "'CODE' is required.")]
	//[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
	//[MaxLength(80)]
	[Column("object_code")]
	public string? ObjectCode { get; set; }

	//[Required(AllowEmptyStrings = false, ErrorMessage = "Item 'NAME' is required.")]
	//[MaxLength(255)]
	[Column("object_name")]
	public string? ObjectName { get; set; }
	[Column("is_deleted")]
	public bool IsDeleted { get; set; }
	[Column("created_user")]
	public string? CreatedUser { get; set; }
	[Column("created_datetime")]
	public DateTime? CreatedDateTime { get; set; }
	[Column("modified_user")]
	public string? ModifiedUser { get; set; }
    [Column("modified_datetime")]

    public DateTime? ModifiedDateTime { get; set; }

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false), ReadOnly(true), NotMapped]
    public string CreatedDateTimeText => CreatedDateTime != null ? CreatedDateTime.Value.ToString("dd-MMM-yyyy HH:mm:ss") : "-";

    [Computed, Write(false), ReadOnly(true), NotMapped]
    public string ModifiedDateTimeText => CreatedDateTime != null ? CreatedDateTime.Value.ToString("dd-MMM-yyyy HH:mm:ss") : "-";

	public static DatabaseObj GetDatabaseObject()
    {
        return new DatabaseObj(SchemaName, MsSqlTableName, PgTableName);
	}

    public static string GetMsSqlTable()
    {
		return string.IsNullOrEmpty(SchemaName) ? $"[{MsSqlTableName}]" : $"[{SchemaName}].[{MsSqlTableName}]";
	}

    public static string GetPgTable()
    {
        return string.IsNullOrEmpty(SchemaName) ? $"\"{PgTableName}\"" : $"{SchemaName}.\"{PgTableName}\"";
	}
	#endregion

	public AuditObject()
    {
        IsDeleted = false;
    }
}