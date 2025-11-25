using DataLayer.Repos.SystemCore;
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
	IUserLocationHistoryRepos UserLocationHistories { get; }
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

    IConnectionFactory Connection { get; }
}

public class UnitOfWork : IUnitOfWork
{
    protected readonly string _appName;
    protected readonly IConnectionFactory _connectionFactory;

    public IConnectionFactory Connection => _connectionFactory;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbConfigs"></param>
    /// <param name="appName"></param>
    /// <param name="datasebType">Default to MS SQL</param>
    /// <exception cref="Exception"></exception>
    public UnitOfWork(IOptionsMonitor<DatabaseConfig> dbConfigs, string appName, string datasebType = DatabaseTypes.MSSQL)
    {
        #region CONNECTION INITIALIZATION
        string appNamePattern = @"^[a-zA-Z0-9-._]{1,}$";

        if (dbConfigs == null)
            throw new Exception($"Missing parameter {nameof(dbConfigs)}");

        if (!Regex.IsMatch(appName, appNamePattern))
            throw new Exception($"Application name specified ('{appName}') is not valid format");

        _appName = appName;

        List<DatabaseConfig> dbConfigList = [];

        DatabaseConfig mongoDbConfig = dbConfigs.Get($"{_appName}MongoDbConnection");

        if (!string.IsNullOrEmpty(mongoDbConfig.DatabaseType) && !string.IsNullOrEmpty(mongoDbConfig.ServerUrl))
            dbConfigList.Add(mongoDbConfig);

        DatabaseConfig azureSqlDbConfig = dbConfigs.Get($"{_appName}SqlConnection");

        if (!string.IsNullOrEmpty(azureSqlDbConfig.DatabaseType) && !string.IsNullOrEmpty(azureSqlDbConfig.ServerUrl))
            dbConfigList.Add(azureSqlDbConfig);

        DatabaseConfig ibmDb2DbConfig = dbConfigs.Get($"{_appName}IbmDb2Connection");

        if (!string.IsNullOrEmpty(ibmDb2DbConfig.DatabaseType) && !string.IsNullOrEmpty(ibmDb2DbConfig.ServerUrl))
            dbConfigList.Add(ibmDb2DbConfig);

        DatabaseConfig posgreSqlDbConfig = dbConfigs.Get($"{_appName}PostgreSqlConnection");

        if (!string.IsNullOrEmpty(posgreSqlDbConfig.DatabaseType) && !string.IsNullOrEmpty(posgreSqlDbConfig.ServerUrl))
            dbConfigList.Add(posgreSqlDbConfig);

        _connectionFactory = new ConnectionFactory(dbConfigList, datasebType);
        #endregion

        #region CORE - Application Core
        Accounts = new AccountRepos(_connectionFactory);
        Addresses = new AddressRepos(_connectionFactory);
        AttachedImages = new AttachedImageRepos(_connectionFactory);
        BusinessEntities = new BusinessEntityRepos(_connectionFactory);
        BusinessSectors = new BusinessSectorRepos(_connectionFactory);
        Calendars = new CalendarRepos(_connectionFactory);
        CambodiaAddresses = new CambodiaAddressRepos(_connectionFactory);
        CambodiaCommunes = new CambodiaCommuneRepos(_connectionFactory);
        CambodiaDistricts = new CambodiaDistrictRepos(_connectionFactory);
        CambodiaProvinces = new CambodiaProvinceRepos(_connectionFactory);
        CambodiaVillages = new CambodiaVillageRepos(_connectionFactory);
		CambodiaCtyStructs = new CambodiaCtyStructRepos(_connectionFactory);
        Contacts = new ContactRepos(_connectionFactory);
        ContactPhones = new ContactPhoneRepos(_connectionFactory);
        Countries = new CountryRepos(_connectionFactory);
        Credentials = new CredentialRepos(_connectionFactory);
        //Databases = new DatabaseRepos(_connectionFactory);
        Documents = new DocumentRepos(_connectionFactory);
        DocumentTypes = new DocumentTypeRepos(_connectionFactory);
        DocumentTemplates = new DocumentTemplateRepos(_connectionFactory);
        DropdownDataLists = new DropdownDataListRepos(_connectionFactory);
        EduFieldOfStudies = new EduFieldOfStudyRepos(_connectionFactory);
        EduQuals = new EduQualRepos(_connectionFactory);
        Industries = new IndustryRepos(_connectionFactory);
        Locations = new LocationRepos(_connectionFactory);
        LocationTypes = new LocationTypeRepos(_connectionFactory);
        LoginHistories = new LoginHistoryRepos(_connectionFactory);
        MasterSettings = new MasterSettingRepos(_connectionFactory);
        MessageLogs = new MessageLogRepos(_connectionFactory);
        MsngrConvoHistories = new MsngrConvoHistoryRepos(_connectionFactory);
        Notifications = new NotificationRepos(_connectionFactory);
        ObjectStateHistories = new ObjectStateHistoryRepos(_connectionFactory);
        ObjectStatusAuditTrails = new ObjectStatusAuditTrailRepos(_connectionFactory);
        Occupations = new OccupationRepos(_connectionFactory);
        OccupationCategories = new OccupationCategoryRepos(_connectionFactory);
        OccupationIndustries = new OccupationIndustryRepos(_connectionFactory);
        Orgs = new OrgRepos(_connectionFactory);
        OrgBranches = new OrgBranchRepos(_connectionFactory);
        OrgStructs = new OrgStructRepos(_connectionFactory);
		OrgStructTypes = new OrgStructTypeRepos(_connectionFactory);
		Permissions = new PermissionRepo(_connectionFactory);
        Persons = new PersonRepos(_connectionFactory);
        ProductCategories = new ProductCategoryRepos(_connectionFactory);
        RolePermissions = new RolePermissionRepos(_connectionFactory);
        Roles = new RoleRepos(_connectionFactory);
        RunNumGenerators = new RunNumGeneratorRepos(_connectionFactory);
        RunNumGenCounters = new RunNumGenCounterRepos(_connectionFactory);
        SysLangLocalizations = new SysLangLocalizationRepos(_connectionFactory);
        SysRunNums = new SysRunNumRepos(_connectionFactory);
        SysObjDocTypes = new SysObjDocTypeRepos(_connectionFactory);
        TelCoExtensions = new TelCoExtensionRepos(_connectionFactory);
        TermAndConditions = new TermAndConditionRepos(_connectionFactory);
        UoMs = new UoMRepos(_connectionFactory);
        UserNotifs = new UserNotifRepos(_connectionFactory);
        UserLocationHistories = new UserLocatinoHistoryRepos(_connectionFactory);
        UserRoles = new UserRoleRepos(_connectionFactory);
        Users = new UserRepos(_connectionFactory);
        UserRoles = new UserRoleRepos(_connectionFactory);
        WorkflowConfigs = new WorkflowConfigRepos(_connectionFactory);
        WorkflowHistories = new WorkflowHistoryRepos(_connectionFactory);
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
    public IUserLocationHistoryRepos UserLocationHistories { get; }
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
            using var cn = Connection.GetDbConnection()!;

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
        _connectionFactory.Dispose();
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        GC.SuppressFinalize(this);
    }

    #endregion
}