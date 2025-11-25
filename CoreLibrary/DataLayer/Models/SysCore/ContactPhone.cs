namespace DataLayer.Models.SysCore;

/// <summary>
/// Align with Pan-Pru API Data Model
/// </summary>
[Table("[dbo].[ContactPhone]")]
public class ContactPhone : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => typeof(ContactPhone).Name;

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"contact_phone";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? LinkedObjectId { get; set; }
    public string? LinkedObjectType { get; set; }

    /// <summary>
    /// Phone Channels
    /// </summary>
    public string? Channel { get; set; }
    public string? CustomChannel { get; set; }
    public string? CountryCode { get; set; }
    public string? CountryCallCode { get; set; }

	/// <summary>
	/// {FK} > DropdownDataList.SystemName=SytemCore, ObjectFieldName='TelecommunicationCompany'
	/// </summary>
	public string? TelCoCode { get; }
    public string? TelcoExtension { get; set; }

    [RegularExpression(@"^[0-9]{0,}$", ErrorMessage = "'Phone Number' invalid format. Please input only number.")]
    public string? PhoneNumber { get; set; }
    public bool IsVerified { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***
    
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, ReadOnly(true)]
    public string FullPhoneNumber
    {
        get
        {
            StringBuilder sb = new();

            if (!string.IsNullOrEmpty(TelcoExtension))
                sb.Append(TelcoExtension + " ");

            sb.Append(PhoneNumber);
            return sb.ToString();
        }
    }
    #endregion

    public void ClearValues()
    {
        Channel = null;
        CustomChannel = null;
        CountryCode = null;
        CountryCallCode = null;
        TelcoExtension = null;
        PhoneNumber = null;
        IsVerified = false;
    }

    public ContactPhone ShallowCopy()
    {
        return (ContactPhone)MemberwiseClone();
    }
}