using DataLayer.Repos.SysCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace DataLayer.Repos;

public interface IUnitOfWork : IDisposable
{
	#region CORE - System Core
	IAddressRepos Addresses { get; }
	IAttachedImageRepos AttachedImages { get; }
	IBusinessEntityRepos BusinessEntities { get; }
	IBusinessSectorRepos BusinessSectors { get; }
	ICalendarRepos Calendars { get; }
	ICambodiaAddressRepos CambodiaAddresses { get; }
	ICambodiaCommuneRepos CambodiaCommunes { get; }
	ICambodiaDistrictRepos CambodiaDistricts { get; }
	ICambodiaProvinceRepos CambodiaProvinces { get; }
	ICambodiaVillageRepos CambodiaVillages { get; }

	/// <summary>
	/// Cambodia Country Structure
	/// </summary>
	ICambodiaCtyStructRepos CambodiaCtyStructs { get; }
	IContactRepos Contacts { get; }
	IContactPhoneRepos ContactPhones { get; }
	ICountryRepos Countries { get; }
	//IDatabaseRepos Databases { get; }
	IDropdownDataListRepos DropdownDataLists { get; }
	IDocumentRepos Documents { get; }
	IDocumentTypeRepos DocumentTypes { get; }
	IDocumentTemplateRepos DocumentTemplates { get; }
	/// <summary>
	/// Education Field of Study
	/// </summary>
	IEduFieldOfStudyRepos EduFieldOfStudies { get; }

	/// <summary>
	/// Education Qualification
	/// </summary>
	IEduQualRepos EduQuals { get; }
	IIndustryRepos Industries { get; }
	ILocationRepos Locations { get; }
	ILocationTypeRepos LocationTypes { get; }
	ILoginHistoryRepos LoginHistories { get; }
	IMasterSettingRepos MasterSettings { get; }
	IMessageLogRepos MessageLogs { get; }
	IMsngrConvoHistoryRepos MsngrConvoHistories { get; }
	INotificationRepos Notifications { get; }
    IObjectStateConfigRepos ObjectStateConfigs { get; }
	IObjectStateHistoryRepos ObjectStateHistories { get; }
	IObjectStatusAuditTrailRepos ObjectStatusAuditTrails { get; }
	IOccupationRepos Occupations { get; }
	IOccupationCategoryRepos OccupationCategories { get; }
	IOccupationIndustryRepos OccupationIndustries { get; }

	/// <summary>
	/// Organization Structure
	/// </summary>
	IOrgStructRepos OrgStructs { get; }
	/// <summary>
	/// Organization Structure Type
	/// </summary>
	IOrgStructTypeRepos OrgStructTypes { get; }

	IPersonRepos Persons { get; }
	IRoleRepos Roles { get; }
	IRolePermissionRepos RolePermissions { get; }
	/// <summary>
	/// Running Number Generator
	/// </summary>
	IRunNumGeneratorRepos RunNumGenerators { get; }

	/// <summary>
	/// Runnning Number Generator Counter
	/// </summary>
	IRunNumGenCounterRepos RunNumGenCounters { get; }

	/// <summary>
	/// System Object Document Type
	/// </summary>
	ISysObjDocTypeRepos SysObjDocTypes { get; }
	/// <summary>
	/// System Running Number
	/// </summary>
	ISysRunNumRepos SysRunNums { get; }
	ITelCoExtensionRepos TelCoExtensions { get; }
	ITermAndConditionRepos TermAndConditions { get; }

    /// <summary>
    /// Organiztaions
    /// </summary>
	IOrgRepos Orgs { get; }

	/// <summary>
	/// Organization Branches
	/// </summary>
	IOrgBranchRepos OrgBranches { get; }

	/// <summary>
	/// System Language Localizations
	/// </summary>
	ISysLangLocalizationRepos SysLangLocalizations { get; }
	IUoMRepos UoMs { get; }
	IUserRepos Users { get; }
	IUserLocHistoryRepos UserLocHistories { get; }
	IUserRoleRepos UserRoles { get; }

    /// <summary>
    /// User Notification
    /// </summary>
	IUserNotifRepos UserNotifs { get; }
	IWorkflowConfigRepos WorkflowConfigs { get; }
	IWorkflowHistoryRepos WorkflowHistories { get; }
	IPermissionRepos Permissions { get; }
	IUserAccountRepos Accounts { get; }
	ICredentialRepos Credentials { get; }
	IProductCategoryRepos ProductCategories { get; set; }
    #endregion

    IDbContext DbContext { get; }
}

public class UnitOfWork : IUnitOfWork
{
    protected readonly string _appName;
    protected readonly IDbContext _dbContext;

    public IDbContext DbContext => _dbContext;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbConfigs"></param>
    /// <param name="appName"></param>
    /// <param name="datasebType">Default to MS SQL</param>
    /// <exception cref="Exception"></exception>
    public UnitOfWork(IOptionsMonitor<DatabaseConfig> dbConfigs, string appName, string dbType = DatabaseTypes.MSSQL)
    {
        #region CONNECTION INITIALIZATION
        string appNamePattern = @"^[a-zA-Z0-9-._]{1,}$";

        if (dbConfigs == null)
            throw new Exception($"Missing parameter {nameof(dbConfigs)}");

        if (!Regex.IsMatch(appName, appNamePattern))
            throw new Exception($"Application name specified ('{appName}') is not valid format");

        _appName = appName;

        DbContext? dbContext = null;

        switch (dbType)
        {
            case DatabaseTypes.MSSQL:
                {
					DatabaseConfig sqlDbConfig = dbConfigs.Get($"{_appName}SqlConnection");
                    if (!string.IsNullOrEmpty(sqlDbConfig.DatabaseType) && !string.IsNullOrEmpty(sqlDbConfig.ServerUrl) && sqlDbConfig.DatabaseType == DatabaseTypes.MSSQL)
                        dbContext = new DbContext(sqlDbConfig);
				}
                break;
			case DatabaseTypes.AZURE_SQL:
				{
					DatabaseConfig sqlDbConfig = dbConfigs.Get($"{_appName}SqlConnection");
					if (!string.IsNullOrEmpty(sqlDbConfig.DatabaseType) && !string.IsNullOrEmpty(sqlDbConfig.ServerUrl) && sqlDbConfig.DatabaseType == DatabaseTypes.AZURE_SQL)
						dbContext = new DbContext(sqlDbConfig);
				}
				break;
			case DatabaseTypes.POSTGRESQL:
				{
					DatabaseConfig sqlDbConfig = dbConfigs.Get($"{_appName}PostgreSqlConnection");
					if (!string.IsNullOrEmpty(sqlDbConfig.DatabaseType) && !string.IsNullOrEmpty(sqlDbConfig.ServerUrl) && sqlDbConfig.DatabaseType == DatabaseTypes.AZURE_SQL)
						dbContext = new DbContext(sqlDbConfig);
				}
                break;
            default:
                throw new Exception($"Database type '{dbType}' is not supported.");
		}
        #endregion

        if (dbContext is null)
            throw new Exception($"Cannot initialize database connection for application '{_appName}' with database type '{dbType}'");
        else
            _dbContext = dbContext;

		#region CORE - Application Core
		Accounts = new AccountRepos(DbContext);
        Addresses = new AddressRepos(DbContext);
        AttachedImages = new AttachedImageRepos(DbContext);
        BusinessEntities = new BusinessEntityRepos(DbContext);
        BusinessSectors = new BusinessSectorRepos(DbContext);
        Calendars = new CalendarRepos(DbContext);
        CambodiaAddresses = new CambodiaAddressRepos(DbContext);
        CambodiaCommunes = new CambodiaCommuneRepos(DbContext);
        CambodiaDistricts = new CambodiaDistrictRepos(DbContext);
        CambodiaProvinces = new CambodiaProvinceRepos(DbContext);
        CambodiaVillages = new CambodiaVillageRepos(DbContext);
		CambodiaCtyStructs = new CambodiaCtyStructRepos(DbContext);
        Contacts = new ContactRepos(DbContext);
        ContactPhones = new ContactPhoneRepos(DbContext);
        Countries = new CountryRepos(DbContext);
        Credentials = new CredentialRepos(DbContext);
        //Databases = new DatabaseRepos(DbContext);
        Documents = new DocumentRepos(DbContext);
        DocumentTypes = new DocumentTypeRepos(DbContext);
        DocumentTemplates = new DocumentTemplateRepos(DbContext);
        DropdownDataLists = new DropdownDataListRepos(DbContext);
        EduFieldOfStudies = new EduFieldOfStudyRepos(DbContext);
        EduQuals = new EduQualRepos(DbContext);
        Industries = new IndustryRepos(DbContext);
        Locations = new LocationRepos(DbContext);
        LocationTypes = new LocationTypeRepos(DbContext);
        LoginHistories = new LoginHistoryRepos(DbContext);
        MasterSettings = new MasterSettingRepos(DbContext);
        MessageLogs = new MessageLogRepos(DbContext);
        MsngrConvoHistories = new MsngrConvoHistoryRepos(DbContext);
        Notifications = new NotificationRepos(DbContext);
		ObjectStateConfigs = new ObjectStateConfigRepos(DbContext);
		ObjectStateHistories = new ObjectStateHistoryRepos(DbContext);
        ObjectStatusAuditTrails = new ObjectStatusAuditTrailRepos(DbContext);
        Occupations = new OccupationRepos(DbContext);
        OccupationCategories = new OccupationCategoryRepos(DbContext);
        OccupationIndustries = new OccupationIndustryRepos(DbContext);
        Orgs = new OrgRepos(DbContext);
        OrgBranches = new OrgBranchRepos(DbContext);
        OrgStructs = new OrgStructRepos(DbContext);
		OrgStructTypes = new OrgStructTypeRepos(DbContext);
		Permissions = new PermissionRepo(DbContext);
        Persons = new PersonRepos(DbContext);
        ProductCategories = new ProductCategoryRepos(DbContext);
        RolePermissions = new RolePermissionRepos(DbContext);
        Roles = new RoleRepos(DbContext);
        RunNumGenerators = new RunNumGeneratorRepos(DbContext);
        RunNumGenCounters = new RunNumGenCounterRepos(DbContext);
        SysLangLocalizations = new SysLangLocalizationRepos(DbContext);
        SysRunNums = new SysRunNumRepos(DbContext);
        SysObjDocTypes = new SysObjDocTypeRepos(DbContext);
        TelCoExtensions = new TelCoExtensionRepos(DbContext);
        TermAndConditions = new TermAndConditionRepos(DbContext);
        UoMs = new UoMRepos(DbContext);
        UserNotifs = new UserNotifRepos(DbContext);
        UserLocHistories = new UserLocHistoryRepos(DbContext);
        UserRoles = new UserRoleRepos(DbContext);
        Users = new UserRepos(DbContext);
        UserRoles = new UserRoleRepos(DbContext);
        WorkflowConfigs = new WorkflowConfigRepos(DbContext);
        WorkflowHistories = new WorkflowHistoryRepos(DbContext);
        #endregion
    }

    #region CORE - Application Core
    public IUserAccountRepos Accounts { get; }
    public IAddressRepos Addresses { get; }
    public IAttachedImageRepos AttachedImages { get; }
    public IBusinessEntityRepos BusinessEntities { get; }
    public IBusinessSectorRepos BusinessSectors { get; }
    public ICalendarRepos Calendars { get; }
    public ICambodiaAddressRepos CambodiaAddresses { get; }
    public ICambodiaCommuneRepos CambodiaCommunes { get; }
    public ICambodiaDistrictRepos CambodiaDistricts { get; }
    public ICambodiaProvinceRepos CambodiaProvinces { get; }
    public ICambodiaVillageRepos CambodiaVillages { get; }
    public ICambodiaCtyStructRepos CambodiaCtyStructs { get; }
    public IContactRepos Contacts { get; }
    public IContactPhoneRepos ContactPhones { get; }
    public ICountryRepos Countries { get; }
    public ICredentialRepos Credentials { get; }
    //public IDatabaseRepos Databases { get; }
    public IDocumentRepos Documents { get; }
    public IDocumentTypeRepos DocumentTypes { get; }
    public IDocumentTemplateRepos DocumentTemplates { get; }
    public IDropdownDataListRepos DropdownDataLists { get; }
    public IEduFieldOfStudyRepos EduFieldOfStudies { get; }
	public IEduQualRepos EduQuals { get; }
    public IIndustryRepos Industries { get; }
    public ILocationRepos Locations { get; }
    public ILocationTypeRepos LocationTypes { get; }
    public ILoginHistoryRepos LoginHistories { get; }
    public IMasterSettingRepos MasterSettings { get; }
    public IOrgRepos Orgs { get; }
    public IPersonRepos Persons { get; set; }
    public IMessageLogRepos MessageLogs { get; }
    public IMsngrConvoHistoryRepos MsngrConvoHistories { get; }
    public INotificationRepos Notifications { get; }
	public IObjectStateConfigRepos ObjectStateConfigs { get; }
	public IObjectStateHistoryRepos ObjectStateHistories { get; }
    public IObjectStatusAuditTrailRepos ObjectStatusAuditTrails { get; }
    public IOccupationRepos Occupations { get; }
    public IOccupationCategoryRepos OccupationCategories { get; }
    public IOccupationIndustryRepos OccupationIndustries { get; }
    public IOrgBranchRepos OrgBranches { get; }
	public IOrgStructRepos OrgStructs { get; }
	public IOrgStructTypeRepos OrgStructTypes { get; }
	public IProductCategoryRepos ProductCategories { get; set; }
    public IRoleRepos Roles { get; }
    public IRolePermissionRepos RolePermissions { get; }
    public IRunNumGeneratorRepos RunNumGenerators { get; }
    public IRunNumGenCounterRepos RunNumGenCounters { get; }
    public ISysLangLocalizationRepos SysLangLocalizations { get; }
    public ISysObjDocTypeRepos SysObjDocTypes { get; }
    public ISysRunNumRepos SysRunNums { get; }
    public ITelCoExtensionRepos TelCoExtensions { get; }
    public ITermAndConditionRepos TermAndConditions { get; }
    public IUoMRepos UoMs { get; }
    public IUserLocHistoryRepos UserLocHistories { get; }
    public IUserNotifRepos UserNotifs { get; }
    public IUserRepos Users { get; }
    public IUserRoleRepos UserRoles { get; }
    public IWorkflowConfigRepos WorkflowConfigs { get; }
    public IWorkflowHistoryRepos WorkflowHistories { get; }
    public IPermissionRepos Permissions { get; }
    #endregion

    public string TestConnection()
    {
        try
        {
            using var cn = DbContext.DbCxn;

            if (cn.State == ConnectionState.Open)
            {
                cn.Close();
            }

            cn.Open();
            return string.Empty;
        }
        catch (Exception ex)
        {
            return ex.GetFullMessage();
        }

    }

    #region IDisposable Support
    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~UnitOfWork() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //_connectionFactory.Dispose();
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        GC.SuppressFinalize(this);
    }

    #endregion
}