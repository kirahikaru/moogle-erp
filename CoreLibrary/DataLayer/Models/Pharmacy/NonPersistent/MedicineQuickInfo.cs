namespace DataLayer.Models.Pharmacy.NonPersistent;

public class MedicineQuickInfo
{
    public int Id { get; set; }
    public string? ObjectCode { get; set; }
    public string? ObjectName { get; set; }
    public string? MfgCountryName { get; set; }
    public string? CompositionSummary { get; set; }
    public int ItemId { get; set; }
    public string? Barcode { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? RetailUnitPrice { get; set; }
    public int? RetailUnitPriceKhr { get; set; }
    public decimal? WholeSaleUnitPrice { get; set; }
    public int? WholeSaleUnitPriceKhr { get; set; }
    public string? InfoLink { get; set; }
    public string? PurchaseLink { get; set; }

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string WholeSaleUnitPriceKhrText
    {
        get
        {
            if (this.WholeSaleUnitPriceKhr.HasValue)
            {
                return $"KHR {this.WholeSaleUnitPriceKhr!.Value:#,##0}";
            }
            else
                return "-";
        }
    }

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string WholeSaleUnitPriceText
    {
        get
        {
            if (!string.IsNullOrEmpty(this.CurrencyCode) && this.WholeSaleUnitPrice.HasValue)
                return $"{this.CurrencyCode} {this.WholeSaleUnitPrice.Value:#,##0.00}";
            else
                return "-";
        }
    }

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string RetailUnitPriceKhrText
    {
        get
        {
            if (this.RetailUnitPriceKhr.HasValue)
                return $"KHR {this.RetailUnitPriceKhr!.Value:#,##0}";
            else
                return "-";
        }
    }

    [Computed]
    [Description("ignore"), ReadOnly(true)]
    public string RetailUnitPriceText
    {
        get
        {
            if (!string.IsNullOrEmpty(this.CurrencyCode) && this.RetailUnitPrice.HasValue)
                return $"{this.CurrencyCode} {this.RetailUnitPrice.Value:#,##0.00}";
            else
                return "-";
        }
    }
}