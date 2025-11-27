using DataLayer.Repos.Pru.Finance;
using DataLayer.Repos.Pru.HR;
using DataLayer.Repos.Pru.IT;
using DataLayer.Repos.Pru.PruCORE;
using Microsoft.Extensions.Options;

namespace DataLayer.Repos;

public interface IUowPruIT : IUnitOfWork
{
	IEmployeeRepos Employees { get; }
	IBudgetItemRepos BudgetItems { get; }
	ICmdbToAppMappingRepos CmdbToAppMappings { get; }
	IExpenseItemRepos ExpenseItems { get; }
	IGLAccountRepos GLAccounts { get; }
	IFinActTrackerRepos FinActivityTrackers { get; }
	IInvoiceRepos Invoices { get; }
	IITAssetRepos ITAssets { get; }
	IITAssetAuditTrailRepos ITAssetAuditTrails { get; }
	IITAssetCategoryRepos ITAssetCategories { get; }
	IPruCoreProjectRepos PruCoreProjects { get; }

	/// <summary>
	/// a.k.a appref
	/// </summary>
	IPruCoreInfraStackRepos InfraStacks { get; }
	IPruLicO365Repos PruLicO365s { get; }
	IPurchaseOrderRepos PurchaseOrders { get; }
	IQuotationRepos Quotations { get; }
	IQuotationItemRepos QuotationItems { get; }
	IVendorRepos Vendors { get; }
}

public class UowPruIT : UnitOfWork, IUowPruIT
{
    public UowPruIT(IOptionsMonitor<DatabaseConfig> dbConfigs, string databaseType = DatabaseTypes.MSSQL) : base(dbConfigs, "PruIT", databaseType)
    {
		Employees = new EmployeeRepos(DbContext);
		BudgetItems = new BudgetItemRepos(DbContext);
		CmdbToAppMappings = new CmdbToAppMappingRepos(DbContext);
		ExpenseItems = new ExpenseItemRepos(DbContext);
		GLAccounts = new GLAccountRepos(DbContext);
		FinActivityTrackers = new FinActTrackerRepos(DbContext);
		Invoices = new InvoiceRepos(DbContext);
		ITAssets = new ITAssetRepos(DbContext);
		ITAssetAuditTrails = new ITAssetAuditTrailRepos(DbContext);
		ITAssetCategories = new ITAssetCategoryRepos(DbContext);
		PruCoreProjects = new PruCoreProjectRepos(DbContext);
		InfraStacks = new PruCoreInfraStackRepos(DbContext);
		PruLicO365s = new PruLicO365Repos(DbContext);
		PurchaseOrders = new PurchaseOrderRepos(DbContext);
		Quotations = new QuotationRepos(DbContext);
		QuotationItems = new QuotationItemRepos(DbContext);
		Vendors = new VendorRepos(DbContext);
	}

	public IEmployeeRepos Employees { get; }
	public IBudgetItemRepos BudgetItems { get; }
	public ICmdbToAppMappingRepos CmdbToAppMappings { get; }
	public IExpenseItemRepos ExpenseItems { get; }
	public IGLAccountRepos GLAccounts { get; }
	public IFinActTrackerRepos FinActivityTrackers { get; }
	public IInvoiceRepos Invoices { get; }
	public IITAssetRepos ITAssets { get; }
	public IITAssetAuditTrailRepos ITAssetAuditTrails { get; }
	public IITAssetCategoryRepos ITAssetCategories { get; }
	public IPruCoreProjectRepos PruCoreProjects { get; }
	
	/// <summary>
	/// a.k.a appref
	/// </summary>
	public IPruCoreInfraStackRepos InfraStacks { get; }
	public IPruLicO365Repos PruLicO365s { get; }
	public IPurchaseOrderRepos PurchaseOrders { get; }
	public IQuotationRepos Quotations { get; }
	public IQuotationItemRepos QuotationItems { get; }
	public IVendorRepos Vendors { get; }
}