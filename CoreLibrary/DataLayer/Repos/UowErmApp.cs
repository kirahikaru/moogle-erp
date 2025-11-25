using DataLayer.Models.Retail;
using DataLayer.Repos.EventManagement;
using DataLayer.Repos.Finance;
using DataLayer.Repos.Hospital;
using DataLayer.Repos.HomeInventory;
using DataLayer.Repos.Pharmacy;
using Microsoft.Extensions.Options;
using DataLayer.Repos.Hobby;
using DataLayer.Repos.SystemCore;
using DataLayer.Repos.Library;
using DataLayer.Repos.Retail;

namespace DataLayer.Repos;

public interface IUowErmApp : IUnitOfWork
{
	#region EMS - Event Management System
	IEventRepos Events { get; }
	IEventInvitationRepos EventInvitations { get; }
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
	IMedicalEquipmentRepos MedicalEquipments { get; }
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
        Events = new EventRepos(_connectionFactory);
        EventOrganizers = new EventOrganizerRepos(_connectionFactory);
        EventOrganizerRoles = new EventOrganizerRoleRepos(_connectionFactory);
        EventInvitations = new EventInvitationRepos(_connectionFactory);
        EventResgistrations = new EventRegistrationRepos(_connectionFactory);
        EventTypes = new EventTypeRepos(_connectionFactory);
        #endregion

        #region FIN - Finance
        Banks = new BankRepos(_connectionFactory);
        Currencies = new CurrencyRepos(_connectionFactory);
        Customers = new CustomerRepos(_connectionFactory);
        ExchangeRates = new ExchangeRateRepos(_connectionFactory);
        Invoices = new InvoiceRepos(_connectionFactory);
        InvoiceItems = new InvoiceItemRepos(_connectionFactory);
        Taxes = new TaxRepos(_connectionFactory);
        TaxRates = new TaxRateRepos(_connectionFactory);
        #endregion

        #region HOME - Home Inventory
        Boardgames = new BoardgameRepos(_connectionFactory);
        BoardgameContentItems = new BoardgameContentItemRepos(_connectionFactory);
        Merchants = new MerchantRepos(_connectionFactory);
        OwnedItems = new OwnedItemRepos(_connectionFactory);
        OwnedItemCategories = new OwnedItemCategoryRepos(_connectionFactory);
        OwnedItemAttachments = new OwnedItemAttachmentRepos(_connectionFactory);
        #endregion

        #region HMS - Healthcare Management System
        Doctors = new DoctorRepos(_connectionFactory);
        HealthcareFacilities = new HealthcareFacilityRepos(_connectionFactory);
        Illnesses = new IllnessRepos(_connectionFactory);
        MedicalExams = new MedicalExamRepos(_connectionFactory);
        MedicalAppointments = new MedicalAppointmentRepos(_connectionFactory);
        MedicalPrescriptions = new MedicalPrescriptionRepos(_connectionFactory);
        MedicalPrescriptionItems = new MedicalPrescriptionItemRepos(_connectionFactory);
        MedicalTests = new MedicalTestRepos(_connectionFactory);
        MedicalTestTypes = new MedicalTestTypeRepos(_connectionFactory);
        Patients = new PatientRepos(_connectionFactory);
        #endregion

        #region HRM - Human Resource Management 
        Employees = new EmployeeRepos(_connectionFactory);

        #endregion

        #region LIB - Library
        BookCategories = new BookCategoryRepos(_connectionFactory);
        BookGenres = new BookGenreRepos(_connectionFactory);
        BookPurchaseHistories = new BookPurchaseHistoryRepos(_connectionFactory);
        Books = new BookRepos(_connectionFactory);
        UserBooks = new UserBookRepos(_connectionFactory);
        #endregion

        #region PMS - Pharmacy Management System
        Medicines = new MedicineRepos(_connectionFactory);
        MedicalEquipments = new MedicalEquipmentRepos(_connectionFactory);
        MedicineCompositions = new MedicineCompositionRepos(_connectionFactory);
        MedicalCompositions = new MedicalCompositionRepos(_connectionFactory);
        #endregion

        #region RMS - Retail Management System
        Brands = new BrandRepos(_connectionFactory);
        CustomerPurchaseInvoices = new CustPurchaseInvoiceRepos(_connectionFactory);
        CustPurchaseInvItems = new CustPurchaseInvItemRepos(_connectionFactory);
        CustomerPurchaseOrders = new CustPurchaseOrderRepos(_connectionFactory);
        CustomerPurchaseOrderItems = new CustPurchaseOrderItemRepos(_connectionFactory);
        CustomerPurchaseInvoicePayments = new CustPurchaseInvPaymentRepos(_connectionFactory);
        DeliveryOptions = new DeliveryOptionRepos(_connectionFactory);
        Gs1CompanyPrefixes = new Gs1CompanyPrefixRepos(_connectionFactory);
        InventoryCheckIns = new InventoryCheckInRepos(_connectionFactory);
        InventoryCheckInItems = new InventoryCheckInItemRepos(_connectionFactory);
        InventoryCheckOuts = new InventoryCheckOutRepos(_connectionFactory);
        InventoryCheckOutItems = new InventoryCheckOutItemRepos(_connectionFactory);
        Items = new ItemRepos(_connectionFactory);
        ItemCategories = new ItemCategoryRepos(_connectionFactory);
        ItemPriceHistories = new ItemPriceHistoryRepos(_connectionFactory);
        ItemVariations = new ItemVariationRepos(_connectionFactory);
        ItemStockBalances = new ItemStockBalanceRepos(_connectionFactory);
        ItemSuppliers = new ItemSupplierRepos(_connectionFactory);
        Manufacturers = new ManufacturerRepos(_connectionFactory);
        Orders = new OrderRepos(_connectionFactory);
        OrderItems = new OrderItemRepos(_connectionFactory);
        Receipts = new ReceiptRepos(_connectionFactory);
        ReceiptItems = new ReceiptItemRepos(_connectionFactory);
        RetailOtherCharges = new RetailOtherChargeRepos(_connectionFactory);
        RetailTaxItems = new RetailTaxItemRepos(_connectionFactory);
        Suppliers = new SupplierRepos(_connectionFactory);
        SupplierBranches = new SupplierBranchRepos(_connectionFactory);
        #endregion
    }

    #region EMS - Event Management System
    public IEventRepos Events { get; }
    public IEventOrganizerRepos EventOrganizers { get; }
    public IEventOrganizerRoleRepos EventOrganizerRoles { get; }
    public IEventInvitationRepos EventInvitations { get; }
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
    public IMedicalEquipmentRepos MedicalEquipments { get; }
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