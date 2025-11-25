namespace DataLayer.Models;

public class AuditObject
{
    public static string SchemaName => "";

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
    public int Id { get; set; }

    //[Required(AllowEmptyStrings = false, ErrorMessage = "'CODE' is required.")]
    //[RegularExpression(@"^[a-zA-Z\d._-]{0,}$", ErrorMessage = "'CODE' invalid format. Valid format input: Capital letter OR number OR . _ - sign")]
    //[MaxLength(80)]
    public string? ObjectCode { get; set; }

    //[Required(AllowEmptyStrings = false, ErrorMessage = "Item 'NAME' is required.")]
    //[MaxLength(255)]
    public string? ObjectName { get; set; }

    public bool IsDeleted { get; set; }

    public string? CreatedUser { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public string? ModifiedUser { get; set; }

    public DateTime? ModifiedDateTime { get; set; }

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false), ReadOnly(true)]
    public string CreatedDateTimeText => CreatedDateTime != null ? CreatedDateTime.Value.ToString("dd-MMM-yyyy HH:mm:ss") : "-";

    [Computed, Write(false), ReadOnly(true)]
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