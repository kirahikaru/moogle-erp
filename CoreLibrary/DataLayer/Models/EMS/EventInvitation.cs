using DataLayer.AuxComponents.DataAnnotations;
using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.EMS;

[Table("[ems].[EventInvitation]"), DisplayName("Event Invitation")]
public class EventInvitation : AuditObject
{
	[Computed, Write(false), ReadOnly(true)]
	public new static string SchemaName => SysDbSchemaNames.EVENT;

    [Computed, Write(false), ReadOnly(true)]
    public new static string MsSqlTableName => typeof(EventInvitation).Name;

	[Computed, Write(false), ReadOnly(true)]
	public new static string PgTableName => "event_invitation";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	[Required]
    public string? LanguageOption { get; set; }

    [MaxLength(150)]
    public string? FullDisplayNameEn { get; set; }

    [MaxLength(150), StringUnicode(true)]
    public string? FullDisplayNameKh { get; set; }

    /// <summary>
    /// Valid Value > Global Constants System Core > Genders
    /// </summary>
    [MaxLength(1)]
    public string? Gender { get; set; }

    /// <summary>
    /// Valid Value > Global Constants System Core > MaritalStatuses
    /// </summary>
    [MaxLength(1)]
    public string? MaritalStatus { get; set; }
    
    [MaxLength(100)]
    public string? CallName { get; set; }

    [RegularExpression(@"^[A-Z\d-]{0,}$", ErrorMessage = "'Invitation ID' invalid format. Valid format input: alpha-numeric")]
    [MaxLength(30)]
    public string? AssignedBarcode { get; set; }

    [Required(ErrorMessage = "Event is required to be selected.")]
    public int? EventId { get; set; }

    /// <summary>
    /// Valid values => GlobalConstant_SystemCore > PersonTitles
    /// </summary>
    [MaxLength(25)]
    public string? NamePrefix { get; set; }

    [MaxLength(25)]
    public string? NameSuffix { get; set; }

    public int? PersonId { get; set; }
    public int? EntityId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? NameLanguage { get; set; }

    public string? GuestOf { get; set; }

    [MaxLength(100)]
    public string? Grouping { get; set; }

    [MaxLength(255)]
    public string? Title { get; set; }

    [MaxLength(255), StringUnicode(true)]
    public string? Remark { get; set; }

    /// <summary>
    /// GlobalConstants_EMS > EventInvitationStatuses
    /// </summary>
    [Required(ErrorMessage = "Invitation Status is required")]
    public string? Status { get; set; }

    /// <summary>
    /// GlobalConstants > Relationships
    /// </summary>
    public string? Relationship { get; set; }
	#endregion

	#region *** LINKED OBJECTS ***
	[Computed, Write(false)]
	public Event? Event { get; set; }

	[Computed, Write(false)]
	public Person? Person { get; set; }

	[Computed, Write(false)]
	public EventRegistration? Registration { get; set; }
	#endregion

	#region *** DYNAMIC PROPERTIES
	[Computed, Write(false), ReadOnly(true)]
	public string StatusText => EventInvitationStatuses.GetDisplayText(Status);

	[Computed, Write(false), ReadOnly(true)]
    public string FullDisplayNameText
    {
        get {
            return NameLanguage switch
            {
                LanguageCodes.ENGLISH => FullDisplayNameEn!,
                LanguageCodes.KHMER => FullDisplayNameKh!,
                _ => "",
            };
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string DisplayName
    {
        get 
        {
            if (Person == null) return "";

            return NameLanguage switch
            {
                LanguageCodes.ENGLISH => FullNameEnText,
                LanguageCodes.KHMER => FullNameKhText,
                _ => "",
            };
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string FullNameEnText
    {
        get {
            StringBuilder sb = new();

            if (!string.IsNullOrEmpty(NamePrefix))
				sb.Append(NamePrefix);

            if (Person != null)
            {
                if (!string.IsNullOrEmpty(Person.Surname))
                    sb.Append(sb.Length == 0 ? Person.Surname : " " + Person.Surname);

                if (!string.IsNullOrEmpty(Person.GivenName))
					sb.Append(sb.Length == 0 ? Person.GivenName : " " + Person.GivenName);
            }

            if (!string.IsNullOrEmpty(NameSuffix))
				sb.Append(sb.Length == 0 ? NameSuffix : " " + NameSuffix);

            return sb.ToString();
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string FullNameKhText
    {
        get 
        {
            StringBuilder sb = new();

            if (!string.IsNullOrEmpty(NamePrefix))
				sb.Append(NamePrefix);

            if (Person != null)
            {
                if (!string.IsNullOrEmpty(Person.Surname))
					sb.Append(sb.Length == 0 ? Person.SurnameKh : " " + Person.SurnameKh);

                if (!string.IsNullOrEmpty(Person.GivenName))
					sb.Append(sb.Length == 0 ? Person.GivenNameKh : " " + Person.GivenNameKh);
            }

            if (!string.IsNullOrEmpty(NameSuffix))
				sb.Append(sb.Length == 0 ? NameSuffix : " " + NameSuffix);

            return sb.ToString();
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string NamePrefixText => PersonTitles.GetDisplayText(NamePrefix);

	[Computed, Write(false), ReadOnly(true)]
	public string GenderText => Genders.GetDisplayText(Gender);

	[Computed, Write(false), ReadOnly(true)]
	public string MaritalStatusText => MaritalStatuses.GetDisplayText(MaritalStatus);

	[Computed, Write(false), ReadOnly(true)]
	public string RelationshipText => Relationships.GetDisplayText(Relationship);

	[Computed, Write(false), ReadOnly(true)]
	public string DisplayNameOnInvitationEn
    {
        get
        {
            StringBuilder sb = new();

            if (!string.IsNullOrEmpty(NamePrefixText))
            {
                sb.Append(NamePrefixText);
            }

            if (!PersonId.HasValue && !string.IsNullOrEmpty(CallName))
            {
                sb.Append(sb.Length > 0 ? $" {CallName}" : CallName);
            }
            else if (PersonId.HasValue && !string.IsNullOrEmpty(FullDisplayNameEn))
            {
                sb.Append(sb.Length > 0 ? $" {FullDisplayNameEn}" : FullDisplayNameEn);
            }

            if (!string.IsNullOrEmpty(NameSuffix))
            {
                sb.Append(sb.Length > 0 ? $" {NameSuffix}" : NameSuffix);
            }

            return sb.ToString();
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string DisplayNameOnInvitationKh
    {
        get
        {
            StringBuilder sb = new();

            string prefixKhText = PersonTitles.GetDisplayTextKh(NamePrefix);

            if (!string.IsNullOrEmpty(prefixKhText))
            {
                sb.Append(prefixKhText);
            }

            if (!PersonId.HasValue && !string.IsNullOrEmpty(CallName))
            {
                sb.Append(sb.Length > 0 ? $" {CallName}" : CallName);
            }
            else if (PersonId.HasValue && !string.IsNullOrEmpty(FullDisplayNameKh))
            {
                sb.Append(sb.Length > 0 ? $" {FullDisplayNameKh}" : FullDisplayNameKh);
            }

            if (!string.IsNullOrEmpty(NameSuffix))
            {
                sb.Append(sb.Length > 0 ? $" {NameSuffix}" : NameSuffix);
            }

            return sb.ToString();
        }
    }

	[Computed, Write(false), ReadOnly(true)]
	public string DualLanguageDisplayNameGender
    {
        get
        {
            return (!string.IsNullOrEmpty(FullDisplayNameEn) ? FullDisplayNameEn : " - ") + " / " + (!string.IsNullOrEmpty(FullDisplayNameKh) ? FullDisplayNameKh : " - ") 
                + " (" + (!string.IsNullOrEmpty(Gender) ? (Genders.GetDisplayText(Gender) + "/" + Genders.GetDispalyTextKh(Gender)) : " - ") + ")";
        }
    }
    #endregion

    public EventInvitation()
    {
        Status = EventInvitationStatuses.PENDING;
        LanguageOption = SystemLocalizationCultures.KHMER;
    }
}