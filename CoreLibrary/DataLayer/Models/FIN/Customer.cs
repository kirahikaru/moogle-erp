using DataLayer.GlobalConstant;

namespace DataLayer.Models.FIN;

[Table("[fin].[Customer]")]
public class Customer : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.FINANCE;

	[Computed, Write(false), ReadOnly(true)]
	public new static string MsSqlTableName => typeof(Customer).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "customer";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	[RegularExpression(@"^[a-zA-Z\W0-9]{0,}$", ErrorMessage = "'NAME' invalid format.")]
    [MaxLength(255)]
    public new string? ObjectName { get; set; }

    public string? ObjectNameKh { get; set; }

    #region *** DATABASE FIELDS ***
    /// <summary>
    /// I : Individual => Person
    /// E : Entity => Business Entity
    /// Valid Values => GlobalConstants_FIN > CustomerTypes
    /// </summary>
    [DefaultValue("I")]
    [Required(ErrorMessage = "'Type' is required.")]
    public string? CustomerType { get; set; }

    public int? PersonId { get; set; }
    public string? PersionCode { get; set; }
	public int? BusinessEntityId { get; set; }
    public string? BusinessEntityCode { get; set; }

	/// <summary>
	/// Valid Values => GlobalConstants_FIN > CustomerStatuses
	/// </summary>
	[Required(ErrorMessage = "'Status' is required.")]
    public string? Status { get; set; }
    public int? RegisteredByUserId { get; set; }

    [Required(ErrorMessage = "'Registration Date' is required.")]
    public DateTime? RegistrationDateTime { get; set; }
    public DateTime? TerminateDateTime { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Person? Person { get; set; }

    [Computed, Write(false)]
    public BusinessEntity? BusinessEntity { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Computed, Write(false)]
    public string NameWithID => $"{ObjectName} ({ObjectCode})";

	[Computed, Write(false)]
	public string CustomerTypeText => CustomerTypes.GetDisplayText(CustomerType);

	[Computed, Write(false)]
	public string PersonGenderText => Person != null ? Person.GenderText : "-";

	[Computed, Write(false)]
	public string PersonBirthDateText => Person != null ? Person.BirthDateText : "-";

	[Computed, Write(false)]
	public string PersonAgeText => Person != null ? Person.AgeText : "-";

	[Computed, Write(false)]
	public string PersonNationalityText => Person != null ? Person.NationalityText : "-";

	[Computed, Write(false)]
	public string BusnRegistrationDateText => BusinessEntity != null ? BusinessEntity.RegistrationDateText : "-";

	[Computed, Write(false)]
	public string BusnLicenceNo => BusinessEntity != null ? BusinessEntity.LicenceNo.NonNullValue("-") : "-";
    #endregion

    public Customer()
    {
        RegistrationDateTime = DateTime.UtcNow.AddHours(7);
        Status = CustomerStatuses.PENDING_ACTIVATION;
    }

    public Customer(string customerType)
    {
        if (!CustomerTypes.IsValid(customerType))
            throw new Exception("Invalid 'Customer Type' specified.");

		RegistrationDateTime = DateTime.UtcNow.AddHours(7);
		Status = CustomerStatuses.PENDING_ACTIVATION;
        CustomerType = customerType;
	}
}