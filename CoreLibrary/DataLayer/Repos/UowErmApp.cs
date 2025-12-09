using DataLayer.Repos.EMS;
using DataLayer.Repos.FIN;
using DataLayer.Repos.HMS;
using DataLayer.Repos.HomeInventory;
using DataLayer.Repos.PMS;
using Microsoft.Extensions.Options;
using DataLayer.Repos.Hobby;
using DataLayer.Repos.SysCore;
using DataLayer.Repos.LIB;
using DataLayer.Repos.RMS;

namespace DataLayer.Repos;

public interface IUowErmApp : IUnitOfWork
{
	#region EMS - Event Management System
	IEventRepos Events { get; }
	IEventInvitRepos EventInvitations { get; }
	IEventOrganizerRepos EventOrganizers { get; }
	IEventOrganizerRoleRepos EventOrganizerRoles { get; }

	IEventRegistrationRepos EventResgistrations { get; }
	IEventTypeRepos EventTypes { get; }
	#endregion

	#region FIN - Finance
	IBankRepos Banks { get; }
	ICurrencyRepos Currencies { get; }
	ICustomerRepos Customers { get; }
	IExchangeRateRepos ExchangeRates { get; }
	IInvoiceRepos Invoices { get; }
	IInvoiceItemRepos InvoiceItems { get; }
	ITaxRepos Taxes { get; }
	ITaxRateRepos TaxRates { get; }
	#endregion

	#region HOME - Home Inventory
	IBoardgameRepos Boardgames { get; }
	IBoardgameContentItemRepos BoardgameContentItems { get; }
	IMerchantRepos Merchants { get; }
	IOwnedItemRepos OwnedItems { get; }
	IOwnedItemCategoryRepos OwnedItemCategories { get; }
	IOwnedItemAttachmentRepos OwnedItemAttachments { get; }
	#endregion

	#region HMS - Healthcare Management System
	IDoctorRepos Doctors { get; }
	IHealthcareFacilityRepos HealthcareFacilities { get; }
	IIllnessRepos Illnesses { get; }
	IMedicalAppointmentRepos MedicalAppointments { get; }
	IMedicalExamRepos MedicalExams { get; }
	IMedicalPrescriptionItemRepos MedicalPrescriptionItems { get; }
	IMedicalPrescriptionRepos MedicalPrescriptions { get; }
	IMedicalTestRepos MedicalTests { get; }
	IMedicalTestTypeRepos MedicalTestTypes { get; }
	IPatientRepos Patients { get; }
	#endregion

	#region HRM - Human Resource Management 
	IEmployeeRepos Employees { get; }

	#endregion

	#region LIB - Library
	IBookRepos Books { get; }
	IBookCategoryRepos BookCategories { get; }
	IBookPurchaseHistoryRepos BookPurchaseHistories { get; }
	IBookGenreRepos BookGenres { get; }
	IUserBookRepos UserBooks { get; }
	#endregion

	#region PMS - Pharmacy Management System
	IMedicineRepos Medicines { get; }
	IMedEquipRepos MedicalEquipments { get; }
	IMedicineCompositionRepos MedicineCompositions { get; }
	IMedicalCompositionRepos MedicalCompositions { get; }
	#endregion

	#region RMS - Retail Management System
	IBrandRepos Brands { get; }
	ICustomerPurchaseInvoiceRepos CustomerPurchaseInvoices { get; }
	ICustPurchaseInvItemRepos CustPurchaseInvItems { get; }
	ICustomerPurchaseOrderRepos CustomerPurchaseOrders { get; }
	ICustomerPurchaseOrderItemRepos CustomerPurchaseOrderItems { get; }
	ICustomerPurchaseInvoicePaymentRepos CustomerPurchaseInvoicePayments { get; }
	IDeliveryOptionRepos DeliveryOptions { get; }
	IGs1CompanyPrefixRepos Gs1CompanyPrefixes { get; }
	IInventoryCheckInRepos InventoryCheckIns { get; }
	IInventoryCheckInItemRepos InventoryCheckInItems { get; }
	IInventoryCheckOutRepos InventoryCheckOuts { get; }
	IInventoryCheckOutItemRepos InventoryCheckOutItems { get; }
	IItemRepos Items { get; }
	IItemCategoryRepos ItemCategories { get; }
	IItemPriceHistoryRepos ItemPriceHistories { get; }
	IItemStockBalanceRepos ItemStockBalances { get; }
	IItemSupplierRepos ItemSuppliers { get; }
	IItemVariationRepos ItemVariations { get; }
	IManufacturerRepos Manufacturers { get; }
	IOrderRepos Orders { get; }
	IOrderItemRepos OrderItems { get; }
	IReceiptRepos Receipts { get; }
	IReceiptItemRepos ReceiptItems { get; }
	IRetailOtherChargeRepos RetailOtherCharges { get; }
	IRetailTaxItemRepos RetailTaxItems { get; }
	ISupplierRepos Suppliers { get; }
	ISupplierBranchRepos SupplierBranches { get; }
	#endregion
}

public class UowErmApp : UnitOfWork, IUowErmApp
{
    public UowErmApp(IOptionsMonitor<DatabaseConfig> dbConfigs) : base(dbConfigs, "ERMApp")
    {
        #region EMS - Event Management System
        Events = new EventRepos(DbContext);
        EventOrganizers = new EventOrganizerRepos(DbContext);
        EventOrganizerRoles = new EventOrganizerRoleRepos(DbContext);
        EventInvitations = new EventInvitRepos(DbContext);
        EventResgistrations = new EventRegistrationRepos(DbContext);
        EventTypes = new EventTypeRepos(DbContext);
        #endregion

        #region FIN - Finance
        Banks = new BankRepos(DbContext);
        Currencies = new CurrencyRepos(DbContext);
        Customers = new CustomerRepos(DbContext);
        ExchangeRates = new ExchangeRateRepos(DbContext);
        Invoices = new InvoiceRepos(DbContext);
        InvoiceItems = new InvoiceItemRepos(DbContext);
        Taxes = new TaxRepos(DbContext);
        TaxRates = new TaxRateRepos(DbContext);
        #endregion

        #region HOME - Home Inventory
        Boardgames = new BoardgameRepos(DbContext);
        BoardgameContentItems = new BoardgameContentItemRepos(DbContext);
        Merchants = new MerchantRepos(DbContext);
        OwnedItems = new OwnedItemRepos(DbContext);
        OwnedItemCategories = new OwnedItemCategoryRepos(DbContext);
        OwnedItemAttachments = new OwnedItemAttachmentRepos(DbContext);
        #endregion

        #region HMS - Healthcare Management System
        Doctors = new DoctorRepos(DbContext);
        HealthcareFacilities = new HealthcareFacilityRepos(DbContext);
        Illnesses = new IllnessRepos(DbContext);
        MedicalExams = new MedExamRepos(DbContext);
        MedicalAppointments = new MedApptRepos(DbContext);
        MedicalPrescriptions = new MedRxRepos(DbContext);
        MedicalPrescriptionItems = new MedApptItemRepos(DbContext);
        MedicalTests = new MedTestRepos(DbContext);
        MedicalTestTypes = new MedTestTypeRepos(DbContext);
        Patients = new PatientRepos(DbContext);
        #endregion

        #region HRM - Human Resource Management 
        Employees = new EmployeeRepos(DbContext);

        #endregion

        #region LIB - Library
        BookCategories = new BookCategoryRepos(DbContext);
        BookGenres = new BookGenreRepos(DbContext);
        BookPurchaseHistories = new BookPurchaseHistoryRepos(DbContext);
        Books = new BookRepos(DbContext);
        UserBooks = new UserBookRepos(DbContext);
        #endregion

        #region PMS - Pharmacy Management System
        Medicines = new MedicineRepos(DbContext);
        MedicalEquipments = new MedEquipRepos(DbContext);
        MedicineCompositions = new MedicineCompositionRepos(DbContext);
        MedicalCompositions = new MedicalCompositionRepos(DbContext);
        #endregion

        #region RMS - Retail Management System
        Brands = new BrandRepos(DbContext);
        CustomerPurchaseInvoices = new CustPurchaseInvoiceRepos(DbContext);
        CustPurchaseInvItems = new CustPurchaseInvItemRepos(DbContext);
        CustomerPurchaseOrders = new CustPurchaseOrderRepos(DbContext);
        CustomerPurchaseOrderItems = new CustPurchaseOrderItemRepos(DbContext);
        CustomerPurchaseInvoicePayments = new CustPurchaseInvPaymentRepos(DbContext);
        DeliveryOptions = new DeliveryOptionRepos(DbContext);
        Gs1CompanyPrefixes = new Gs1CompanyPrefixRepos(DbContext);
        InventoryCheckIns = new InventoryCheckInRepos(DbContext);
        InventoryCheckInItems = new InventoryCheckInItemRepos(DbContext);
        InventoryCheckOuts = new InventoryCheckOutRepos(DbContext);
        InventoryCheckOutItems = new InventoryCheckOutItemRepos(DbContext);
        Items = new ItemRepos(DbContext);
        ItemCategories = new ItemCategoryRepos(DbContext);
        ItemPriceHistories = new ItemPriceHistoryRepos(DbContext);
        ItemVariations = new ItemVariationRepos(DbContext);
        ItemStockBalances = new ItemStockBalanceRepos(DbContext);
        ItemSuppliers = new ItemSupplierRepos(DbContext);
        Manufacturers = new ManufacturerRepos(DbContext);
        Orders = new OrderRepos(DbContext);
        OrderItems = new OrderItemRepos(DbContext);
        Receipts = new ReceiptRepos(DbContext);
        ReceiptItems = new ReceiptItemRepos(DbContext);
        RetailOtherCharges = new RetailOtherChargeRepos(DbContext);
        RetailTaxItems = new RetailTaxItemRepos(DbContext);
        Suppliers = new SupplierRepos(DbContext);
        SupplierBranches = new SupplierBranchRepos(DbContext);
        #endregion
    }

    #region EMS - Event Management System
    public IEventRepos Events { get; }
    public IEventOrganizerRepos EventOrganizers { get; }
    public IEventOrganizerRoleRepos EventOrganizerRoles { get; }
    public IEventInvitRepos EventInvitations { get; }
    public IEventRegistrationRepos EventResgistrations { get; }
    public IEventTypeRepos EventTypes { get; }
    #endregion

    #region FIN - Finance
    public IBankRepos Banks { get; }
    public ICurrencyRepos Currencies { get; }
    public ICustomerRepos Customers { get; }
    public IExchangeRateRepos ExchangeRates { get; }
    public IInvoiceRepos Invoices { get; }
    public IInvoiceItemRepos InvoiceItems { get; }
    public ITaxRepos Taxes { get; }
    public ITaxRateRepos TaxRates { get; }
	#endregion

	#region HMS - Healthcare Management System
	public IDoctorRepos Doctors { get; }
    public IIllnessRepos Illnesses { get; }
	public IHealthcareFacilityRepos HealthcareFacilities { get; }
	public IMedicalAppointmentRepos MedicalAppointments { get; }
	public IMedicalExamRepos MedicalExams { get; }
	public IMedicalPrescriptionItemRepos MedicalPrescriptionItems { get; }
	public IMedicalPrescriptionRepos MedicalPrescriptions { get; }
	public IMedicalTestRepos MedicalTests { get; }
	public IMedicalTestTypeRepos MedicalTestTypes { get; }
    public IPatientRepos Patients { get; }
    #endregion

    #region HOME - Home Inventory
    public IBoardgameRepos Boardgames { get; }
    public IBoardgameContentItemRepos BoardgameContentItems { get; }
    public IMerchantRepos Merchants { get; }
    public IOwnedItemRepos OwnedItems { get; }
    public IOwnedItemCategoryRepos OwnedItemCategories { get; }
    public IOwnedItemAttachmentRepos OwnedItemAttachments { get; }
    #endregion

    #region HRM - Human Resource Management 
    public IEmployeeRepos Employees { get; }

    #endregion

    #region LIB - Library
    public IBookRepos Books { get; }
    public IBookCategoryRepos BookCategories { get; }
    public IBookPurchaseHistoryRepos BookPurchaseHistories { get; }
    public IBookGenreRepos BookGenres { get; }
    public IUserBookRepos UserBooks { get; }
    #endregion

    #region PMS - Pharmacy Management System
    public IMedicineRepos Medicines { get; }
    public IMedEquipRepos MedicalEquipments { get; }
    public IMedicineCompositionRepos MedicineCompositions { get; }
    public IMedicalCompositionRepos MedicalCompositions { get; }
    #endregion

    #region RMS - Retail Management System
    public IBrandRepos Brands { get; }
    public ICustomerPurchaseInvoiceRepos CustomerPurchaseInvoices { get; }
    public ICustPurchaseInvItemRepos CustPurchaseInvItems { get; }
    public ICustomerPurchaseOrderRepos CustomerPurchaseOrders { get; }
    public ICustomerPurchaseOrderItemRepos CustomerPurchaseOrderItems { get; }
    public ICustomerPurchaseInvoicePaymentRepos CustomerPurchaseInvoicePayments { get; }
    public IDeliveryOptionRepos DeliveryOptions { get; }
    public IGs1CompanyPrefixRepos Gs1CompanyPrefixes { get; }
    public IInventoryCheckInRepos InventoryCheckIns { get; }
    public IInventoryCheckInItemRepos InventoryCheckInItems { get; }
    public IInventoryCheckOutRepos InventoryCheckOuts { get; }
    public IInventoryCheckOutItemRepos InventoryCheckOutItems { get; }
    public IItemRepos Items { get; }
    public IItemCategoryRepos ItemCategories { get; }
    public IItemPriceHistoryRepos ItemPriceHistories { get; }
    public IItemStockBalanceRepos ItemStockBalances { get; }
    public IItemSupplierRepos ItemSuppliers { get; }
	public IItemVariationRepos ItemVariations { get; }
	public IManufacturerRepos Manufacturers { get; }
    public IOrderRepos Orders { get; }
    public IOrderItemRepos OrderItems { get; }
    public IReceiptRepos Receipts { get; }
    public IReceiptItemRepos ReceiptItems { get; }
    public IRetailOtherChargeRepos RetailOtherCharges { get; }
    public IRetailTaxItemRepos RetailTaxItems { get; }
    public ISupplierRepos Suppliers { get; }
    public ISupplierBranchRepos SupplierBranches { get; }
    #endregion
}